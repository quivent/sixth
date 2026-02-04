\ expected: 156566624
\ Hamming distance - count differing bits between two numbers

: hamming ( a b -- distance )
  xor
  0 swap
  begin dup while
    dup 1 and rot + swap
    1 rshift
  repeat drop ;

: main
  0
  5000 0 do
    5000 0 do
      i j hamming +
    loop
  loop
  . cr ;
