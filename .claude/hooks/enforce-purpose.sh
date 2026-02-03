#!/bin/bash
# Enforcement hook: Prevent unauthorized work on the compiler
# The agent must get explicit user approval before modifying compiler code
# or running compiler-related commands.
#
# Also injects a reminder of the PRIMARY PURPOSE into every tool call.

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name')
TOOL_INPUT=$(echo "$INPUT" | jq -r '.tool_input')

REMINDER="REMINDER: PRIMARY PURPOSE is compile speed faster than GCC -O2, runtime at parity or better. Is this action serving that purpose? If not, did the user explicitly request it? If both NO, STOP and ask the user."

# Extract relevant fields based on tool
case "$TOOL_NAME" in
  Edit|Write)
    FILE_PATH=$(echo "$TOOL_INPUT" | jq -r '.file_path // empty')

    # Block edits to compiler source without explicit approval
    if [[ "$FILE_PATH" == *"compiler/sixth.fs"* ]] || [[ "$FILE_PATH" == *"compiler/"*".fs" ]]; then
      jq -n --arg reminder "$REMINDER" '{
        "hookSpecificOutput": {
          "hookEventName": "PreToolUse",
          "permissionDecision": "ask",
          "permissionDecisionReason": "ENFORCEMENT: Modifying compiler source requires explicit user approval. Does this change directly improve COMPILE SPEED or RUNTIME SPEED? If not, do not proceed.",
          "additionalContext": $reminder
        }
      }'
      exit 0
    fi

    # For non-compiler edits, still inject the reminder
    jq -n --arg reminder "$REMINDER" '{
      "hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "additionalContext": $reminder
      }
    }'
    exit 0
    ;;

  Bash)
    COMMAND=$(echo "$TOOL_INPUT" | jq -r '.command // empty')

    # ALLOW: compilation, benchmarks, verification - these serve the purpose
    # Block: investigation loops, debugging tangents, exploration without direction

    # Block repeated investigation patterns (valgrind, strace, gdb without explicit request)
    if [[ "$COMMAND" == *"valgrind"* ]] || [[ "$COMMAND" == *"strace"* ]] || [[ "$COMMAND" == *"gdb"* ]]; then
      jq -n --arg reminder "$REMINDER" '{
        "hookSpecificOutput": {
          "hookEventName": "PreToolUse",
          "permissionDecision": "ask",
          "permissionDecisionReason": "ENFORCEMENT: Debugging tools (valgrind/strace/gdb) require explicit user request. Are you investigating instead of building?",
          "additionalContext": $reminder
        }
      }'
      exit 0
    fi

    # For other bash commands, still inject the reminder
    jq -n --arg reminder "$REMINDER" '{
      "hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "additionalContext": $reminder
      }
    }'
    exit 0
    ;;
esac

# Default: inject reminder
jq -n --arg reminder "$REMINDER" '{
  "hookSpecificOutput": {
    "hookEventName": "PreToolUse",
    "additionalContext": $reminder
  }
}'
exit 0
