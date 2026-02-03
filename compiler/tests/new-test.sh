#!/bin/bash
# new-test.sh - Create a new compiler test with required headers
# Usage: ./compiler/tests/new-test.sh <number> <name> <category> <reason>
#
# This is the ONLY way to add tests. Direct file creation is forbidden.

set -e

if [ $# -lt 4 ]; then
    echo "Usage: $0 <number> <name> <category> <reason>"
    echo ""
    echo "Categories:"
    echo "  codegen      - Tests assembly output for primitives"
    echo "  optimization - Tests compiler optimizations"
    echo "  fold         - Tests constant folding"
    echo "  fuse         - Tests instruction fusing"
    echo "  fwd-ref      - Tests forward references"
    echo "  regression   - Tests for specific bug fixes"
    echo "  sixth-word   - Tests words unique to Sixth"
    echo "  integration  - Tests real algorithms"
    echo ""
    echo "Example:"
    echo "  $0 1050 fold-lshift fold 'Verify left-shift folding at compile time'"
    echo ""
    echo "FORBIDDEN: Tests for standard Forth behavior. Hayes tests cover those."
    exit 1
fi

NUM="$1"
NAME="$2"
CATEGORY="$3"
REASON="$4"

# Validate category
case "$CATEGORY" in
    codegen|optimization|fold|fuse|fwd-ref|regression|sixth-word|integration)
        ;;
    *)
        echo "ERROR: Invalid category '$CATEGORY'"
        echo "Valid: codegen optimization fold fuse fwd-ref regression sixth-word integration"
        exit 1
        ;;
esac

# Check for forbidden patterns in reason
REASON_LOWER=$(echo "$REASON" | tr '[:upper:]' '[:lower:]')
for PATTERN in "test addition" "test subtraction" "test if" "test loop" "test do" "test stack" "test arithmetic"; do
    if echo "$REASON_LOWER" | grep -q "$PATTERN"; then
        echo "ERROR: Reason contains forbidden pattern '$PATTERN'"
        echo ""
        echo "This sounds like you're testing standard Forth behavior."
        echo "Hayes tests already cover standard Forth. Do not duplicate."
        echo ""
        echo "If you're testing CODEGEN for these operations, phrase it as:"
        echo "  'Verify X generates correct assembly' or"
        echo "  'Verify optimization Y eliminates Z'"
        exit 1
    fi
done

OUTFILE="compiler/tests/${NUM}-${NAME}.fs"

if [ -f "$OUTFILE" ]; then
    echo "ERROR: $OUTFILE already exists"
    exit 1
fi

cat > "$OUTFILE" << EOF
\\ expect:
\\ category: $CATEGORY
\\ reason: $REASON

: main
  \\ TODO: implement test
;
EOF

echo "Created: $OUTFILE"
echo ""
echo "Now edit the file to:"
echo "  1. Fill in expected output after 'expect:'"
echo "  2. Implement the test in 'main'"
echo ""
echo "Run with: ./engine/fifth compiler/sixth.fs $OUTFILE /tmp/t && /tmp/t"
