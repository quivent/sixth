\ Sieve of Eratosthenes benchmark
100001 constant N
create sieve N allot

: init-sieve
  sieve N 1 fill
  0 sieve c!
  0 sieve 1+ c! ;

: mark-composite ( start step -- )
  N 1- swap do
    0 sieve i + c!
  dup +loop drop ;

: sieve-pass ( i -- )
  dup dup * N < if
    sieve over + c@ if
      dup dup * swap mark-composite
    else drop then
  else drop then ;

: run-sieve
  init-sieve
  N 2 do i sieve-pass loop ;

: count-primes ( -- n )
  0 N 2 do sieve i + c@ + loop ;

: main
  run-sieve
  count-primes . cr ;
