\ expect: 42
\ Edge case test: Chained arithmetic that accumulates errors
\
\ Tests accumulation of intermediate results through many operations.
\ This can expose:
\ - Register allocation bugs (values clobbered)
\ - Stack corruption
\ - Incorrect operand ordering in binary operations
\
\ The chain: start with 100, apply many operations, arrive at 42
\
\ 100 -> various ops -> 42
\
\ Path:
\   100
\   + 50 = 150
\   * 2 = 300
\   - 100 = 200
\   / 4 = 50
\   + 10 = 60
\   mod 42 = 18 (60 mod 42 = 18)
\   + 24 = 42

: add50 ( n -- n ) 50 + ;
: mul2 ( n -- n ) 2 * ;
: sub100 ( n -- n ) 100 - ;
: div4 ( n -- n ) 4 / ;
: add10 ( n -- n ) 10 + ;
: mod42 ( n -- n ) 42 mod ;
: add24 ( n -- n ) 24 + ;

: chain1 ( n -- n )
  add50 mul2 ;

: chain2 ( n -- n )
  sub100 div4 ;

: chain3 ( n -- n )
  add10 mod42 add24 ;

: main
  \ Long chain through helper words
  100
  chain1   \ 100 + 50 = 150, * 2 = 300
  chain2   \ 300 - 100 = 200, / 4 = 50
  chain3   \ 50 + 10 = 60, mod 42 = 18, + 24 = 42

  \ Verify with parallel computation and subtraction
  \ If we recompute the same chain inline, subtract should give 0
  100 50 + 2 * 100 - 4 / 10 + 42 mod 24 +
  -            \ Should be 42 - 42 = 0
  42 +         \ 0 + 42 = 42
;
