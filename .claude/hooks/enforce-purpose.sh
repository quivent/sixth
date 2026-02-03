#!/bin/bash
# Enforcement hook: Prevent unauthorized work on the compiler
# The agent must get explicit user approval before modifying compiler code
# or running compiler-related commands.

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name')
TOOL_INPUT=$(echo "$INPUT" | jq -r '.tool_input')

# Extract relevant fields based on tool
case "$TOOL_NAME" in
  Edit|Write)
    FILE_PATH=$(echo "$TOOL_INPUT" | jq -r '.file_path // empty')

    # Block edits to compiler source without explicit approval
    if [[ "$FILE_PATH" == *"compiler/sixth.fs"* ]] || [[ "$FILE_PATH" == *"compiler/"*".fs" ]]; then
      jq -n '{
        "hookSpecificOutput": {
          "hookEventName": "PreToolUse",
          "permissionDecision": "ask",
          "permissionDecisionReason": "ENFORCEMENT: Modifying compiler source requires explicit user approval. Does this change directly improve COMPILE SPEED or RUNTIME SPEED? If not, do not proceed."
        }
      }'
      exit 0
    fi
    ;;

  Bash)
    COMMAND=$(echo "$TOOL_INPUT" | jq -r '.command // empty')

    # Block test suite runs without approval
    if [[ "$COMMAND" == *"test"* ]] && [[ "$COMMAND" == *"compiler"* ]]; then
      jq -n '{
        "hookSpecificOutput": {
          "hookEventName": "PreToolUse",
          "permissionDecision": "ask",
          "permissionDecisionReason": "ENFORCEMENT: Running compiler tests requires explicit user request. Did the user ask for this?"
        }
      }'
      exit 0
    fi

    # Block self-hosting attempts without approval
    if [[ "$COMMAND" == *"sixth"* ]] && [[ "$COMMAND" == *"sixth.fs"* ]] && [[ "$COMMAND" == *"sixth.fs"*"sixth.fs"* ]]; then
      jq -n '{
        "hookSpecificOutput": {
          "hookEventName": "PreToolUse",
          "permissionDecision": "ask",
          "permissionDecisionReason": "ENFORCEMENT: Self-hosting attempts require explicit user request. Did the user ask to work on self-hosting?"
        }
      }'
      exit 0
    fi
    ;;
esac

# Allow everything else
exit 0
