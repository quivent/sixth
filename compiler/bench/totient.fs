\ expected: 30393485
\ Euler's totient function - count coprime numbers

: gcd ( a b -- gcd )
  begin dup while
    tuck mod
  repeat drop ;

: totient ( n -- phi )
  dup 1 do
    dup i gcd 1 = if swap 1+ swap then
  loop drop ;

: main
  0
  10000 1 do
    i totient +
  loop
  . cr ;
