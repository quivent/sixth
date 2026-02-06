#!/bin/bash
# Run adversarial loop tests for ARM64 compiler

COMPILER="./engine/fifth compiler/shannon-arm64.fs"
TESTDIR="tools/arm64-tests"
PASS=0
FAIL=0
CFAIL=0

for test in "$TESTDIR"/adversarial-loop-*.fs; do
    name=$(basename "$test")
    expected=$(grep -m1 '^\\* expect:' "$test" 2>/dev/null | sed 's/.*expect: *//')
    if [ -z "$expected" ]; then
        expected=$(grep -m1 '^\\ expect:' "$test" 2>/dev/null | sed 's/.*expect: *//')
    fi

    rm -f /tmp/test-out

    # Compile
    compile_out=$($COMPILER "$test" /tmp/test-out 2>&1)
    compile_rc=$?

    if [ $compile_rc -ne 0 ]; then
        echo "CFAIL: $name (compile failed: $compile_out)"
        ((CFAIL++))
        continue
    fi

    # Run with timeout
    timeout 5 /tmp/test-out
    actual=$?

    if [ "$actual" = "$expected" ]; then
        echo "PASS: $name (expected $expected, got $actual)"
        ((PASS++))
    else
        echo "FAIL: $name (expected $expected, got $actual)"
        ((FAIL++))
    fi
done

echo ""
echo "===== SUMMARY ====="
echo "PASS: $PASS"
echo "FAIL: $FAIL"
echo "CFAIL: $CFAIL"
echo "TOTAL: $((PASS + FAIL + CFAIL))"
