#!/bin/bash
# Test Oracle for Sixth Compiler
# Compares interpreter vs compiled output for every primitive
#
# This finds actual BUGS - cases where both interpreter and compiler
# accept the same syntax but produce different results.
#
# Words that only exist in one or the other are noted but not failures.

cd /home/josh/quivent/fifth

PASS=0
FAIL=0
INTERP_ONLY=0
COMPILER_ONLY=0
FAILED_TESTS=""
INTERP_ONLY_TESTS=""
COMPILER_ONLY_TESTS=""

# Test function: compare interpreter vs compiled output
# All code is wrapped in : main ... ; to ensure consistent behavior
test_prim() {
    local name="$1"
    local code="$2"

    # For interpreter: define main and call it
    echo ": main $code ; main" > /tmp/t.fs
    interp_out=$(./fifth /tmp/t.fs 2>&1)
    interp_err=$?

    # Check if interpreter had undefined word error
    if echo "$interp_out" | grep -q "ABORT: undefined word"; then
        # Interpreter doesn't have this word - not a bug in compiler
        # But check if compiler has it
        echo ": main $code ;" > /tmp/tc.fs
        compile_out=$(./fifth compiler/sixth.fs /tmp/tc.fs /tmp/t 2>&1)
        if [ $? -eq 0 ]; then
            echo "SKIP: $name (compiler-only word)"
            COMPILER_ONLY=$((COMPILER_ONLY + 1))
            COMPILER_ONLY_TESTS="$COMPILER_ONLY_TESTS\n$name"
        else
            echo "SKIP: $name (neither interpreter nor compiler has this)"
        fi
        return
    fi

    # For compiler: just define main (no call - compiled binary auto-runs main)
    echo ": main $code ;" > /tmp/tc.fs
    compile_out=$(./fifth compiler/sixth.fs /tmp/tc.fs /tmp/t 2>&1)
    compile_status=$?

    if [ $compile_status -ne 0 ]; then
        # Check if compile error mentions undefined word
        if echo "$compile_out" | grep -q "undefined word\|ABORT"; then
            echo "SKIP: $name (interpreter-only word)"
            INTERP_ONLY=$((INTERP_ONLY + 1))
            INTERP_ONLY_TESTS="$INTERP_ONLY_TESTS\n$name"
            return
        fi
        echo "FAIL: $name (compile error)"
        echo "  Code: $code"
        echo "  Compile error: $compile_out"
        FAIL=$((FAIL + 1))
        FAILED_TESTS="$FAILED_TESTS\n$name: compile error"
        return
    fi

    compiled_out=$(/tmp/t 2>&1)
    run_status=$?

    if [ "$interp_out" = "$compiled_out" ]; then
        echo "PASS: $name"
        PASS=$((PASS + 1))
    else
        echo "FAIL: $name"
        echo "  Code: $code"
        echo "  Interpreter: '$interp_out'"
        echo "  Compiled:    '$compiled_out'"
        FAIL=$((FAIL + 1))
        FAILED_TESTS="$FAILED_TESTS\n$name"
    fi
}

# Test full program (code includes multiple definitions)
# Interpreter: code + main call
# Compiler: code only (no main call at end, binary auto-runs)
test_prog() {
    local name="$1"
    local interp_code="$2"
    local comp_code="$3"

    # If only 2 args, use same code for both but add/remove main call
    if [ -z "$comp_code" ]; then
        interp_code="$2"
        # Remove trailing "main" from code for compiler
        comp_code=$(echo "$2" | sed 's/ main$//')
    fi

    # Write for interpreter (with main call)
    echo "$interp_code" > /tmp/t.fs
    interp_out=$(./fifth /tmp/t.fs 2>&1)

    # Check if interpreter had undefined word error
    if echo "$interp_out" | grep -q "ABORT: undefined word"; then
        echo "SKIP: $name (interpreter missing word)"
        INTERP_ONLY=$((INTERP_ONLY + 1))
        return
    fi

    # Write for compiler (no main call - binary auto-runs main)
    echo "$comp_code" > /tmp/tc.fs
    compile_out=$(./fifth compiler/sixth.fs /tmp/tc.fs /tmp/t 2>&1)
    if [ $? -ne 0 ]; then
        if echo "$compile_out" | grep -q "undefined word\|ABORT"; then
            echo "SKIP: $name (compiler missing word)"
            COMPILER_ONLY=$((COMPILER_ONLY + 1))
            return
        fi
        echo "FAIL: $name (compile error)"
        echo "  Compile error: $compile_out"
        FAIL=$((FAIL + 1))
        FAILED_TESTS="$FAILED_TESTS\n$name: compile error"
        return
    fi

    compiled_out=$(/tmp/t 2>&1)

    if [ "$interp_out" = "$compiled_out" ]; then
        echo "PASS: $name"
        PASS=$((PASS + 1))
    else
        echo "FAIL: $name"
        echo "  Interpreter code: $interp_code"
        echo "  Interpreter: '$interp_out'"
        echo "  Compiled:    '$compiled_out'"
        FAIL=$((FAIL + 1))
        FAILED_TESTS="$FAILED_TESTS\n$name"
    fi
}

echo "=========================================="
echo "=== Arithmetic Operations ==="
echo "=========================================="
test_prim "add (+)" "3 5 + . cr"
test_prim "subtract (-)" "10 3 - . cr"
test_prim "multiply (*)" "6 7 * . cr"
test_prim "divide (/)" "42 6 / . cr"
test_prim "modulo (mod)" "17 5 mod . cr"
test_prim "negate" "42 negate . cr"
test_prim "1+" "41 1+ . cr"
test_prim "1-" "43 1- . cr"
test_prim "2*" "21 2* . cr"
test_prim "2/" "84 2/ . cr"
test_prim "abs positive" "42 abs . cr"
test_prim "abs negative" "-42 abs . cr"
test_prim "max" "3 7 max . cr"
test_prim "min" "3 7 min . cr"

echo ""
echo "=========================================="
echo "=== Comparison Operations ==="
echo "=========================================="
test_prim "equal (=) true" "5 5 = . cr"
test_prim "equal (=) false" "5 6 = . cr"
test_prim "less than (<) true" "3 5 < . cr"
test_prim "less than (<) false" "5 3 < . cr"
test_prim "greater than (>) true" "5 3 > . cr"
test_prim "greater than (>) false" "3 5 > . cr"
test_prim "less or equal (<=) less" "3 5 <= . cr"
test_prim "less or equal (<=) equal" "5 5 <= . cr"
test_prim "greater or equal (>=) greater" "5 3 >= . cr"
test_prim "greater or equal (>=) equal" "5 5 >= . cr"
test_prim "0= true" "0 0= . cr"
test_prim "0= false" "5 0= . cr"
test_prim "0< true" "-5 0< . cr"
test_prim "0< false" "5 0< . cr"
test_prim "0> true" "5 0> . cr"
test_prim "0> false" "-5 0> . cr"
test_prim "<> true" "5 6 <> . cr"
test_prim "<> false" "5 5 <> . cr"
test_prim "u< unsigned less" "1 2 u< . cr"
test_prim "u> unsigned greater" "2 1 u> . cr"

echo ""
echo "=========================================="
echo "=== Stack Operations ==="
echo "=========================================="
test_prim "dup" "42 dup . . cr"
test_prim "drop" "1 2 drop . cr"
test_prim "swap" "1 2 swap . . cr"
test_prim "over" "1 2 over . . . cr"
test_prim "rot" "1 2 3 rot . . . cr"
test_prim "-rot" "1 2 3 -rot . . . cr"
test_prim "nip" "1 2 nip . cr"
test_prim "tuck" "1 2 tuck . . . cr"
test_prim "2dup" "1 2 2dup . . . . cr"
test_prim "2drop" "1 2 3 4 2drop . . cr"
test_prim "2swap" "1 2 3 4 2swap . . . . cr"
test_prim "2over" "1 2 3 4 2over . . . . . . cr"
test_prim "pick 0" "1 2 3 0 pick . . . . cr"
test_prim "pick 1" "1 2 3 1 pick . . . . cr"
test_prim "pick 2" "1 2 3 2 pick . . . . cr"
test_prim "?dup nonzero" "5 ?dup . . cr"
test_prim "?dup zero" "0 ?dup . cr"
test_prim "depth" "1 2 3 depth . . . . cr"

echo ""
echo "=========================================="
echo "=== Logic Operations ==="
echo "=========================================="
test_prim "and" "15 7 and . cr"
test_prim "or" "8 7 or . cr"
test_prim "xor" "15 9 xor . cr"
test_prim "invert" "0 invert . cr"
test_prim "lshift" "1 4 lshift . cr"
test_prim "rshift" "16 2 rshift . cr"

echo ""
echo "=========================================="
echo "=== Memory Operations ==="
echo "=========================================="
test_prog "@ !" \
    "variable x 42 x ! : main x @ . cr ; main" \
    "variable x 42 x ! : main x @ . cr ;"
test_prog "c@ c!" \
    "create buf 1 allot : main 65 buf c! buf c@ . cr ; main" \
    "create buf 1 allot : main 65 buf c! buf c@ . cr ;"
test_prog "+!" \
    "variable x 10 x ! : main 5 x +! x @ . cr ; main" \
    "variable x 10 x ! : main 5 x +! x @ . cr ;"
test_prim "cells" "3 cells . cr"
test_prim "cell+" "8 cell+ . cr"
test_prim "chars" "3 chars . cr"
test_prim "char+" "8 char+ . cr"
test_prog "here allot" \
    "create buf 10 allot : main here buf - . cr ; main" \
    "create buf 10 allot : main here buf - . cr ;"

echo ""
echo "=========================================="
echo "=== Control Flow ==="
echo "=========================================="
test_prim "if true" "1 if 42 . then cr"
test_prim "if false" "0 if 42 . then cr"
test_prim "if-else true" "1 if 42 . else 99 . then cr"
test_prim "if-else false" "0 if 42 . else 99 . then cr"
test_prim "nested if true-true" "1 if 1 if 42 . then then cr"
test_prim "nested if true-false" "1 if 0 if 42 . then 99 . then cr"
test_prim "nested if false" "0 if 1 if 42 . then then 99 . cr"
test_prog "exit from word" \
    ": f 1 . exit 2 . ; : main f cr ; main" \
    ": f 1 . exit 2 . ; : main f cr ;"

echo ""
echo "=========================================="
echo "=== Loop Constructs ==="
echo "=========================================="
test_prim "begin-until" "0 begin 1+ dup 5 = until . cr"
test_prim "begin-while-repeat" "0 begin dup 5 < while 1+ repeat . cr"
test_prog "do-loop count" \
    ": main 0 5 0 do 1+ loop . cr ; main" \
    ": main 0 5 0 do 1+ loop . cr ;"
test_prog "do-loop i" \
    ": main 5 0 do i . loop cr ; main" \
    ": main 5 0 do i . loop cr ;"
test_prog "do-loop sum" \
    ": main 0 5 0 do i + loop . cr ; main" \
    ": main 0 5 0 do i + loop . cr ;"
test_prog "do-+loop" \
    ": main 10 0 do i . 2 +loop cr ; main" \
    ": main 10 0 do i . 2 +loop cr ;"
test_prog "nested do-loop" \
    ": main 3 0 do 3 0 do i j + . loop loop cr ; main" \
    ": main 3 0 do 3 0 do i j + . loop loop cr ;"
test_prog "?do zero iterations" \
    ": main 0 0 0 ?do 1+ loop . cr ; main" \
    ": main 0 0 0 ?do 1+ loop . cr ;"
test_prog "?do with iterations" \
    ": main 0 5 0 ?do 1+ loop . cr ; main" \
    ": main 0 5 0 ?do 1+ loop . cr ;"
test_prog "leave" \
    ": main 10 0 do i 5 = if leave then i . loop cr ; main" \
    ": main 10 0 do i 5 = if leave then i . loop cr ;"
test_prog "unloop exit" \
    ": f 10 0 do i 5 = if unloop exit then i . loop ; : main f cr ; main" \
    ": f 10 0 do i 5 = if unloop exit then i . loop ; : main f cr ;"

echo ""
echo "=========================================="
echo "=== Return Stack ==="
echo "=========================================="
test_prim ">r r>" "42 >r r> . cr"
test_prim "r@" "42 >r r@ . r> drop cr"
test_prim "2>r 2r>" "1 2 2>r 2r> . . cr"
test_prim "2r@" "1 2 2>r 2r@ . . 2r> 2drop cr"

echo ""
echo "=========================================="
echo "=== Helper Word Calls ==="
echo "=========================================="
test_prog "simple helper" \
    ": double 2 * ; : main 21 double . cr ; main" \
    ": double 2 * ; : main 21 double . cr ;"
test_prog "helper chain" \
    ": a 1 + ; : b a a ; : main 10 b . cr ; main" \
    ": a 1 + ; : b a a ; : main 10 b . cr ;"
test_prog "multi arg helper" \
    ": add3 + + ; : main 1 2 3 add3 . cr ; main" \
    ": add3 + + ; : main 1 2 3 add3 . cr ;"
test_prog "helper with if" \
    ": sign dup 0< if drop -1 else dup 0> if drop 1 else drop 0 then then ; : main 5 sign . -3 sign . 0 sign . cr ; main" \
    ": sign dup 0< if drop -1 else dup 0> if drop 1 else drop 0 then then ; : main 5 sign . -3 sign . 0 sign . cr ;"

# Bug pattern tests: helper calls with extra stack items
test_prog "2-arg helper with extra stack" \
    ": a + ; : main 10 20 30 a . . cr ; main" \
    ": a + ; : main 10 20 30 a . . cr ;"
test_prog "3-arg helper with extra stack" \
    ": a + + ; : main 10 20 30 40 a . . cr ; main" \
    ": a + + ; : main 10 20 30 40 a . . cr ;"
test_prog "two helper calls in sequence" \
    ": a + ; : main 1 2 3 a a . cr ; main" \
    ": a + ; : main 1 2 3 a a . cr ;"
test_prog "two different helpers" \
    ": a + ; : b * ; : main 1 2 3 a b . cr ; main" \
    ": a + ; : b * ; : main 1 2 3 a b . cr ;"

echo ""
echo "=========================================="
echo "=== Recursion ==="
echo "=========================================="
# Note: Compiler requires 'recursive' keyword before recurse
test_prog "recurse factorial (interp style)" \
    ": fact dup 1 > if dup 1- recurse * then ; : main 5 fact . cr ; main" \
    ": fact recursive dup 1 > if dup 1- recurse * then ; : main 5 fact . cr ;"
test_prog "recurse fibonacci (interp style)" \
    ": fib dup 2 < if else dup 1- recurse swap 2 - recurse + then ; : main 10 fib . cr ; main" \
    ": fib recursive dup 2 < if exit then dup 1- recurse swap 2 - recurse + ; : main 10 fib . cr ;"

echo ""
echo "=========================================="
echo "=== Advanced Arithmetic ==="
echo "=========================================="
test_prim "*/ (star-slash)" "10 3 2 */ . cr"
test_prim "*/mod" "10 3 2 */mod . . cr"
test_prim "/mod" "17 5 /mod . . cr"
test_prim "s>d" "42 s>d . . cr"
test_prim "m*" "1000 1000 m* . . cr"

echo ""
echo "=========================================="
echo "=== Miscellaneous ==="
echo "=========================================="
test_prim "true constant" "true . cr"
test_prim "false constant" "false . cr"
test_prim "bl (blank)" "bl . cr"

echo ""
echo "=========================================="
echo "=== SUMMARY ==="
echo "=========================================="
echo "PASS: $PASS"
echo "FAIL: $FAIL"
echo "Compiler-only words: $COMPILER_ONLY"
echo "Interpreter-only words: $INTERP_ONLY"
echo ""
if [ $FAIL -gt 0 ]; then
    echo "=========================================="
    echo "FAILED TESTS (BUGS):"
    echo "=========================================="
    echo -e "$FAILED_TESTS"
fi
if [ $COMPILER_ONLY -gt 0 ]; then
    echo ""
    echo "Compiler-only words (not bugs):"
    echo -e "$COMPILER_ONLY_TESTS"
fi
