\ expect: 128
\ NEGATE of MIN_INT64 - signed overflow
\ -MIN_INT64 cannot be represented, wraps to MIN_INT64
\ Tests that codegen handles this correctly
: main 1 63 lshift negate 56 rshift ;
