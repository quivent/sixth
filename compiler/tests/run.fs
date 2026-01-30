\ run.fs - Run all tests
\ Usage: fifth compiler/tests/run.fs

s" echo '=== COMPILER TESTS ===' && for f in compiler/tests/[0-9]*.fs; do name=$(basename $f .fs); echo -n \"$name: \"; ./fifth compiler/tf.fs $f /tmp/test-$name 2>/dev/null && /tmp/test-$name 2>&1 | tr -d '\\n' && echo '' || echo 'FAIL'; done" system

bye
