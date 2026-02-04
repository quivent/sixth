\ expected: -2872092127636481573
\ Fibonacci with memoization, fib(10000) mod 2^63 (wraps)

10001 constant MEMO-SIZE
create memo MEMO-SIZE cells allot

: fibmemo-init ( -- )
  MEMO-SIZE 0 do 0 memo i cells + ! loop
  1 memo 1 cells + ! ;

: fibmemo ( n -- f )
  dup 2 < if exit then
  2 begin
    dup 3 pick <= while
    dup 1- cells memo + @
    over 2 - cells memo + @
    + over cells memo + !
    1+
  repeat drop
  cells memo + @ ;

: main
  fibmemo-init
  10000 fibmemo . cr ;
