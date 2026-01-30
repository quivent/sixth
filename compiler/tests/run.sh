#!/bin/bash
# Test runner for Fifth compiler tests
# Writes results to /tmp/fifth-test-results.txt
cd "$(dirname "$0")/../.."

RESULTS=/tmp/fifth-test-results.txt
> "$RESULTS"

pass=0; fail=0; cfail=0; total=0

for f in compiler/tests/[0-9]*.fs; do
    name=$(basename "$f" .fs)
    total=$((total+1))
    bin="/tmp/t-$name"
    rm -f "$bin"

    # Compile with timing
    t0=$(date +%s%N)
    cout=$(timeout 5 ./fifth compiler/tf.fs "$f" "$bin" 2>&1)
    crc=$?
    t1=$(date +%s%N)
    cms=$(( (t1 - t0) / 1000000 ))

    if [ $crc -ne 0 ] || [ ! -f "$bin" ]; then
        cfail=$((cfail+1))
        reason=$(echo "$cout" | grep -v '^$' | head -1)
        echo "CFAIL ${cms}ms $name: $reason" >> "$RESULTS"
        continue
    fi

    # Run
    rout=$(timeout 2 "$bin" 2>&1)
    rrc=$?

    if [ $rrc -ne 0 ]; then
        fail=$((fail+1))
        echo "RFAIL ${cms}ms rc=$rrc $name: got='$(echo "$rout" | head -1)'" >> "$RESULTS"
    else
        pass=$((pass+1))
        echo "PASS  ${cms}ms $name" >> "$RESULTS"
    fi
done

echo "" >> "$RESULTS"
echo "TOTAL: $total  PASS: $pass  CFAIL: $cfail  RFAIL: $fail" >> "$RESULTS"

echo "TOTAL: $total  PASS: $pass  CFAIL: $cfail  RFAIL: $fail"
echo "Details: $RESULTS"
