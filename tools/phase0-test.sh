#!/bin/bash
# phase0-test.sh - Phase gate test for ARM64 Mach-O reference binary
#
# Tests tools/macho-test.c in all three modes:
#   1. exit 42
#   2. arith (6*7=42)
#   3. hello (prints "Hello\n")
#
# Also verifies Mach-O structure with otool and dyld_info.
# Exit 0 = all pass, exit 1 = failure.

set -e
cd "$(dirname "$0")/.."

PASS=0
FAIL=0
MACHO_TEST=./tools/macho-test

# Colors (if terminal supports them)
if [ -t 1 ]; then
    GREEN='\033[0;32m'
    RED='\033[0;31m'
    RESET='\033[0m'
else
    GREEN=''
    RED=''
    RESET=''
fi

pass() {
    echo -e "  ${GREEN}PASS${RESET} $1"
    PASS=$((PASS + 1))
}

fail() {
    echo -e "  ${RED}FAIL${RESET} $1: $2"
    FAIL=$((FAIL + 1))
}

# Ensure macho-test binary exists
if [ ! -x "$MACHO_TEST" ]; then
    echo "Building tools/macho-test..."
    cc -o tools/macho-test tools/macho-test.c
fi

echo "Phase 0: ARM64 Mach-O Reference Binary"
echo "======================================="
echo

# Test 1: exit 42
echo "Test 1: exit 42"
$MACHO_TEST exit 42 /tmp/phase0-exit42
chmod +x /tmp/phase0-exit42
codesign -f -s - /tmp/phase0-exit42 2>/dev/null
EXIT_CODE=0
/tmp/phase0-exit42 || EXIT_CODE=$?
if [ "$EXIT_CODE" = "42" ]; then
    pass "exit code = 42"
else
    fail "exit code" "expected 42, got $EXIT_CODE"
fi

# Test 2: arith (6*7=42)
echo "Test 2: arith (6*7=42)"
$MACHO_TEST arith 0 /tmp/phase0-arith
chmod +x /tmp/phase0-arith
codesign -f -s - /tmp/phase0-arith 2>/dev/null
EXIT_CODE=0
/tmp/phase0-arith || EXIT_CODE=$?
if [ "$EXIT_CODE" = "42" ]; then
    pass "arith exit code = 42"
else
    fail "arith exit code" "expected 42, got $EXIT_CODE"
fi

# Test 3: hello
echo "Test 3: hello world"
$MACHO_TEST hello 0 /tmp/phase0-hello
chmod +x /tmp/phase0-hello
codesign -f -s - /tmp/phase0-hello 2>/dev/null
OUTPUT=$(/tmp/phase0-hello)
if [ "$OUTPUT" = "Hello" ]; then
    pass "hello output = 'Hello'"
else
    fail "hello output" "expected 'Hello', got '$OUTPUT'"
fi

# Test 4: Mach-O structure verification
echo "Test 4: Mach-O structure"
OTOOL_OUT=$(otool -l /tmp/phase0-exit42 2>&1)

if echo "$OTOOL_OUT" | grep -q "LC_MAIN"; then
    pass "LC_MAIN present"
else
    fail "LC_MAIN" "not found in load commands"
fi

if echo "$OTOOL_OUT" | grep -q "LC_DYLD_CHAINED_FIXUPS"; then
    pass "LC_DYLD_CHAINED_FIXUPS present"
else
    fail "LC_DYLD_CHAINED_FIXUPS" "not found in load commands"
fi

if echo "$OTOOL_OUT" | grep -q "LC_DYLD_EXPORTS_TRIE"; then
    pass "LC_DYLD_EXPORTS_TRIE present"
else
    fail "LC_DYLD_EXPORTS_TRIE" "not found in load commands"
fi

if echo "$OTOOL_OUT" | grep -q "__LINKEDIT"; then
    pass "__LINKEDIT segment present"
else
    fail "__LINKEDIT" "segment not found"
fi

# Test 5: Exports verification
echo "Test 5: exports"
EXPORTS_OUT=$(dyld_info -exports /tmp/phase0-exit42 2>&1)
if echo "$EXPORTS_OUT" | grep -q "_main"; then
    pass "_main exported"
else
    fail "_main" "not found in exports"
fi

if echo "$EXPORTS_OUT" | grep -q "__mh_execute_header"; then
    pass "__mh_execute_header exported"
else
    fail "__mh_execute_header" "not found in exports"
fi

# Cleanup
rm -f /tmp/phase0-exit42 /tmp/phase0-arith /tmp/phase0-hello

# Summary
echo
echo "======================================="
echo "Phase 0: $PASS passed, $FAIL failed"

if [ "$FAIL" -gt 0 ]; then
    echo -e "${RED}PHASE 0 FAILED${RESET}"
    exit 1
else
    echo -e "${GREEN}PHASE 0 PASSED${RESET}"
    exit 0
fi
