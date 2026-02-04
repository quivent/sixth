\ expected: 6020697
\ Pollard's rho - find factors using cycle detection

: gcd ( a b -- gcd )
  begin dup while tuck mod repeat drop ;

: abs ( n -- |n| )
  dup 0< if negate then ;

: pollard-f ( x n -- x' )
  swap dup * 1+ swap mod ;

: pollard-rho ( n -- factor )
  dup 1 and 0= if drop 2 exit then
  2 2 rot                 \ x=2, y=2, n
  begin
    over 2 pick pollard-f rot drop swap  \ x'=f(x), y, n
    over 2 pick pollard-f 2 pick pollard-f
    rot drop swap         \ x, y'=f(f(y)), n
    2dup - abs 2 pick gcd \ x y n d
    dup 1 = while drop
  repeat
  nip nip nip ;

: main
  \ Sum smallest factors of odd numbers
  0
  10000 3 do
    i pollard-rho +
  2 +loop
  . cr ;
