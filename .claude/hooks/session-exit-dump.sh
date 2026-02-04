#!/bin/bash
# Session Exit Hook - Dump debugging state on session end
# Ensures no work is lost even on manual session termination
#
# Creates a timestamped snapshot of current state

INPUT=$(cat)
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"
DEBUG_STATE="$PROJECT_DIR/compiler/shannon/DEBUG-STATE.md"
DUMP_DIR="$PROJECT_DIR/.claude/session-dumps"

# Create dump directory if needed
mkdir -p "$DUMP_DIR"

DUMP_FILE="$DUMP_DIR/session-$TIMESTAMP.md"

{
    echo "# SESSION DUMP - $TIMESTAMP"
    echo "# Auto-generated on session exit"
    echo ""
    echo "## Session Info"
    echo ""
    echo "- Timestamp: $(date)"
    echo "- Working Directory: $PROJECT_DIR"
    echo "- Git Branch: $(cd "$PROJECT_DIR" && git branch --show-current 2>/dev/null || echo 'unknown')"
    echo "- Git Commit: $(cd "$PROJECT_DIR" && git rev-parse --short HEAD 2>/dev/null || echo 'unknown')"
    echo ""

    # Dump DEBUG-STATE.md if it exists
    if [[ -f "$DEBUG_STATE" ]]; then
        echo "## DEBUG-STATE.md (at exit)"
        echo ""
        echo '```markdown'
        cat "$DEBUG_STATE"
        echo '```'
        echo ""
    else
        echo "## DEBUG-STATE.md"
        echo ""
        echo "No debug state file found."
        echo ""
    fi

    # Dump uncommitted changes
    echo "## Uncommitted Changes"
    echo ""
    echo '```'
    cd "$PROJECT_DIR" && git status --short 2>/dev/null || echo "Git status unavailable"
    echo '```'
    echo ""

    # Dump recent git log
    echo "## Recent Commits"
    echo ""
    echo '```'
    cd "$PROJECT_DIR" && git log --oneline -10 2>/dev/null || echo "Git log unavailable"
    echo '```'
    echo ""

    # Check for any modified .fs files in compiler/shannon
    echo "## Modified Shannon Files"
    echo ""
    MODIFIED=$(cd "$PROJECT_DIR" && git diff --name-only compiler/shannon/*.fs 2>/dev/null)
    if [[ -n "$MODIFIED" ]]; then
        echo "Modified files:"
        echo '```'
        echo "$MODIFIED"
        echo '```'
        echo ""
        echo "### Diffs"
        echo ""
        for file in $MODIFIED; do
            echo "#### $file"
            echo '```diff'
            cd "$PROJECT_DIR" && git diff "$file" 2>/dev/null | head -100
            echo '```'
            echo ""
        done
    else
        echo "No modified shannon compiler files."
    fi
    echo ""

    # Final reminder
    echo "## Resume Instructions"
    echo ""
    echo "To resume this debugging session:"
    echo ""
    echo "1. Read \`compiler/shannon/DEBUG-STATE.md\`"
    echo "2. Run \`/debug-channel --resume\`"
    echo "3. Check this dump file for any uncommitted changes"
    echo ""
    echo "---"
    echo "*Dump created by session-exit-dump.sh hook*"

} > "$DUMP_FILE"

# Also update DEBUG-STATE.md with session end marker if it exists
if [[ -f "$DEBUG_STATE" ]]; then
    # Append session end marker
    echo "" >> "$DEBUG_STATE"
    echo "---" >> "$DEBUG_STATE"
    echo "**SESSION ENDED: $TIMESTAMP** - Dump saved to \`.claude/session-dumps/session-$TIMESTAMP.md\`" >> "$DEBUG_STATE"
fi

# Stop hook uses simple schema - just output a reason
jq -n --arg file "$DUMP_FILE" '{
    "stopReason": ("Session state dumped to: " + $file)
}'
exit 0
