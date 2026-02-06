\ expect: 0
\ Brutal Integration Test 06: Prime Sieve
\ Tests: bit manipulation, loops, memory, conditionals

variable sieve-base

: bit-addr ( n -- byte-addr bit-mask )
  dup 8 / sieve-base @ +
  swap 7 and 1 swap lshift ;

: set-comp ( n -- )
  bit-addr over c@ or swap c! ;

: prime? ( n -- flag )
  bit-addr swap c@ and 0= ;

: init-sieve ( -- )
  here sieve-base ! 32 allot
  sieve-base @ 32 0 fill ;

: mark-mult ( prime -- )
  dup dup *
  begin
    dup 256 < while
    dup set-comp
    over +
  repeat
  2drop ;

: run-sieve ( -- )
  init-sieve
  0 set-comp
  1 set-comp
  256 2 do
    i prime? if i mark-mult then
  loop ;

: count-primes ( limit -- count )
  0 swap
  2 do
    i prime? if 1+ then
  loop ;

: main
  run-sieve
  2 prime? 0= if 1 exit then
  17 prime? 0= if 1 exit then
  97 prime? 0= if 1 exit then
  4 prime? if 1 exit then
  100 prime? if 1 exit then
  50 count-primes 15 <> if 1 exit then
  0 ;
