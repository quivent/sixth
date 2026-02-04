#!/bin/bash
# Debug Channel Enforcement Hook
# Enforces the /debug-channel protocol when debugging compiler issues
#
# KEY ENFORCEMENT: Parallel agents MUST be the first action

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name')
TOOL_INPUT=$(echo "$INPUT" | jq -r '.tool_input')

# Check if we're in a debug context (DEBUG-STATE.md exists)
DEBUG_STATE="$CLAUDE_PROJECT_DIR/compiler/shannon/DEBUG-STATE.md"

if [[ ! -f "$DEBUG_STATE" ]]; then
    # Not in debug mode, pass through
    echo '{}'
    exit 0
fi

# We're in debug mode - enforce the protocol

# Extract description/content for pattern matching
DESCRIPTION=$(echo "$TOOL_INPUT" | jq -r '.description // .content // .command // empty' 2>/dev/null)
DESCRIPTION_LOWER=$(echo "$DESCRIPTION" | tr '[:upper:]' '[:lower:]')

# ============================================================================
# PARALLEL FIRST ENFORCEMENT
# ============================================================================

# Check if this is a Task tool call (launching agents)
if [[ "$TOOL_NAME" == "Task" ]]; then
    # Good - they're launching an agent. Allow it.
    jq -n '{
        "hookSpecificOutput": {
            "hookEventName": "PreToolUse",
            "additionalContext": "PARALLEL AGENT LAUNCH: Good. Make sure you launch at least 3 agents for independent hypotheses."
        }
    }'
    exit 0
fi

# Check if this is a Read of DEBUG-STATE.md (allowed as prep for parallel)
if [[ "$TOOL_NAME" == "Read" ]]; then
    FILE_PATH=$(echo "$TOOL_INPUT" | jq -r '.file_path // empty')
    if [[ "$FILE_PATH" == *"DEBUG-STATE"* ]]; then
        jq -n '{
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "additionalContext": "Reading DEBUG-STATE.md - Good. After reading, IMMEDIATELY launch 3 parallel agents. Do not investigate sequentially."
            }
        }'
        exit 0
    fi
fi

# For any OTHER tool during debug mode, remind about parallel first
PARALLEL_REMINDER="⚠️ PARALLEL FIRST: Per /debug-channel protocol, your FIRST action must be launching 3 parallel agents to test hypotheses independently. Are you launching parallel agents? If not, you are doing sequential debugging which has failed repeatedly on this bug."

# ============================================================================
# BANNED PHRASE DETECTION
# ============================================================================

BANNED_PATTERNS=(
    "root cause found"
    "found it"
    "this is definitely"
    "the bug is"
    "fixed!"
    "i found the"
    "the problem is"
    "the issue is"
    "this fixes"
    "that's the bug"
)

for pattern in "${BANNED_PATTERNS[@]}"; do
    if [[ "$DESCRIPTION_LOWER" == *"$pattern"* ]]; then
        jq -n --arg pattern "$pattern" --arg parallel "$PARALLEL_REMINDER" '{
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "permissionDecision": "ask",
                "permissionDecisionReason": ("DEBUG-CHANNEL VIOLATION: Banned phrase detected: \"" + $pattern + "\". You cannot claim to have found the bug without verification. Rephrase as: \"H[N] is now at [X]% confidence\". Have you run parallel agents? Verified with disassembly? Run the failing test?"),
                "additionalContext": $parallel
            }
        }'
        exit 0
    fi
done

# ============================================================================
# BACKTRACK PHRASE DETECTION
# ============================================================================

BACKTRACK_PATTERNS=(
    "but wait"
    "actually,"
    "oh, i see"
    "that's not it"
    "wait, "
    "hmm, that"
    "no, the real"
    "i was wrong"
)

for pattern in "${BACKTRACK_PATTERNS[@]}"; do
    if [[ "$DESCRIPTION_LOWER" == *"$pattern"* ]]; then
        jq -n --arg pattern "$pattern" '{
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "permissionDecision": "ask",
                "permissionDecisionReason": ("DEBUG-CHANNEL VIOLATION: Backtrack phrase detected: \"" + $pattern + "\". Your previous hypothesis was wrong. STOP. Update DEBUG-STATE.md with failed hypothesis. If backtrack count >= 3, your mental model is fundamentally wrong - run /refresh."),
                "additionalContext": "BACKTRACK DETECTED: You were wrong. This is expected - but you must log it and update probabilities before continuing."
            }
        }'
        exit 0
    fi
done

# ============================================================================
# SEQUENTIAL INVESTIGATION WARNING
# ============================================================================

# Detect patterns that suggest sequential investigation instead of parallel
SEQUENTIAL_PATTERNS=(
    "let me check"
    "let me look"
    "let me investigate"
    "let me understand"
    "let me read"
    "let me see"
    "i'll check"
    "i'll look"
    "first, let me"
    "first let me"
)

for pattern in "${SEQUENTIAL_PATTERNS[@]}"; do
    if [[ "$DESCRIPTION_LOWER" == *"$pattern"* ]]; then
        jq -n --arg pattern "$pattern" '{
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "permissionDecision": "ask",
                "permissionDecisionReason": ("SEQUENTIAL DEBUGGING DETECTED: \"" + $pattern + "\" suggests you are investigating sequentially. Per /debug-channel, you MUST launch 3 parallel agents FIRST. Every previous agent investigated sequentially and failed. Are you launching parallel agents right now?"),
                "additionalContext": "PARALLEL IS MANDATORY: Launch 3 agents for H1, H2, H3 simultaneously. Do not investigate one hypothesis at a time."
            }
        }'
        exit 0
    fi
done

# ============================================================================
# STANDARD DEBUG MODE REMINDER
# ============================================================================

jq -n --arg parallel "$PARALLEL_REMINDER" '{
    "hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "additionalContext": $parallel
    }
}'
exit 0
