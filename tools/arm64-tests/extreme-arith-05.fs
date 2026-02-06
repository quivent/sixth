\ expect: 128
\ ABS of MIN_INT64 - another signed overflow case
\ |MIN_INT64| cannot be represented, returns MIN_INT64
\ Tests CNEG instruction behavior
: main 1 63 lshift abs 56 rshift ;
