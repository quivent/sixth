\ expect: 2 3 5 7 11 13 17 19 23 29 31 37 41 43 47
create sieve 51 allot
variable pp
variable mm
: init-sieve 51 0 do 1 sieve i + c! loop  0 sieve c!  0 sieve 1 + c! ;
: mark-mult ( p -- )
  dup pp !  dup * mm !
  begin mm @ 51 < while
    0 sieve mm @ + c!
    pp @ mm +!
  repeat ;
: do-sieve
  init-sieve
  8 2 do
    sieve i + c@ if i mark-mult then
  loop ;
: print-primes
  51 2 do
    sieve i + c@ if i . then
  loop cr ;
: main do-sieve print-primes ;
