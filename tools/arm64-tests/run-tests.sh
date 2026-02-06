#!/bin/bash
# ARM64 compiler test runner

PASS=0
FAIL=0
TOTAL=0

for test in tools/arm64-tests/phase*.fs; do
    TOTAL=$((TOTAL + 1))
    name=$(basename "$test" .fs)
    
    # Extract expected - check if it's a string or exit code
    expected=$(grep '^\\ expect:' "$test" | sed 's/\\ expect: //')
    if [ -z "$expected" ]; then
        expected=0
    fi
    
    # Compile
    ./engine/fifth compiler/shannon-arm64.fs "$test" /tmp/arm64-test 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "FAIL $name: compilation failed"
        FAIL=$((FAIL + 1))
        continue
    fi
    
    # Binary is already signed by save-binary, no additional signing needed
    
    # Run and capture output
    output=$(/tmp/arm64-test 2>&1)
    actual_code=$?
    
    # Check if expected is numeric (exit code test) or string (output test)
    if [[ "$expected" =~ ^[0-9]+$ ]]; then
        # Exit code test
        if [ "$actual_code" = "$expected" ]; then
            echo "PASS $name"
            PASS=$((PASS + 1))
        else
            echo "FAIL $name: expected exit $expected, got $actual_code"
            FAIL=$((FAIL + 1))
        fi
    else
        # Output test - compare trimmed output
        output_trimmed=$(echo "$output" | tr -d '\n' | sed 's/^ *//;s/ *$//')
        expected_trimmed=$(echo "$expected" | tr -d '\n' | sed 's/^ *//;s/ *$//')
        if [ "$output_trimmed" = "$expected_trimmed" ]; then
            echo "PASS $name"
            PASS=$((PASS + 1))
        else
            echo "FAIL $name: expected '$expected_trimmed', got '$output_trimmed'"
            FAIL=$((FAIL + 1))
        fi
    fi
done

echo ""
echo "TOTAL: $TOTAL  PASS: $PASS  FAIL: $FAIL"
