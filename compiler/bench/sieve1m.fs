\ expected: 1229
\ Sieve of Eratosthenes - count primes up to 10,000

10001 constant LIMIT
create sieve LIMIT allot

: init-sieve ( -- )
  LIMIT 0 do 1 sieve i + c! loop
  0 sieve c!  0 sieve 1+ c! ;

: mark-multiples ( n -- )
  dup dup *
  begin dup LIMIT < while
    0 over sieve + c!
    over +
  repeat 2drop ;

: run-sieve ( -- )
  2 begin dup dup * LIMIT < while
    dup sieve + c@ if dup mark-multiples then
    1+
  repeat drop ;

: count-primes ( -- n )
  0 LIMIT 2 do sieve i + c@ if 1+ then loop ;

: main init-sieve run-sieve count-primes . cr ;
