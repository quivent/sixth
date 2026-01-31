\ expect: 25
create sieve 101 allot
variable pp
variable mm
: init-sieve 101 0 do 1 sieve i + c! loop  0 sieve c!  0 sieve 1 + c! ;
: mark-mult ( p -- )
  dup pp !  dup * mm !
  begin mm @ 101 < while
    0 sieve mm @ + c!
    pp @ mm +!
  repeat ;
: do-sieve
  init-sieve
  11 2 do
    sieve i + c@ if i mark-mult then
  loop ;
: count-primes ( -- n )
  0  101 2 do
    sieve i + c@ if 1+ then
  loop ;
: main do-sieve count-primes . cr ;
