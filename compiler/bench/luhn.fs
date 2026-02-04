\ expected: 200000
\ Luhn checksum benchmark - validate credit card numbers, run 100000 times

create testnum 16 allot

: init-num ( -- )
  s" 4532015112830366" drop testnum 16 cmove ;

: digit-at ( i -- d )
  testnum + c@ [char] 0 - ;

: luhn-check ( -- valid )
  0
  16 0 do
    15 i - digit-at
    i 2 mod if
      2* dup 9 > if 9 - then
    then
    +
  loop
  10 mod 0= ;

: main ( -- )
  init-num
  0
  100000 0 do
    luhn-check if 2 + else 1+ then
  loop
  . cr ;
