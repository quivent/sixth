\ expected: 5000000
\ Gray code - convert binary to Gray code and back

: to-gray ( n -- gray )
  dup 1 rshift xor ;

: from-gray ( gray -- n )
  dup
  begin dup 1 rshift dup while
    rot xor swap
  repeat drop ;

: main
  0
  5000000 0 do
    i to-gray from-gray
    i = if 1+ then
  loop
  . cr ;
