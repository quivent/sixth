\ expect: 42
\ Multiplication overflow wraps to zero
\ 2^62 * 4 = 2^64 = 0 (mod 2^64)
\ Tests MUL instruction behavior on overflow
: main 1 62 lshift 4 * 42 + ;
