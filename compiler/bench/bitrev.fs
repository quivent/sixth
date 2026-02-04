\ expected: 0
\ Bit reverse - reverse bits in a 32-bit number

: bitrev32 ( n -- reversed )
  0 32 0 do
    1 lshift
    over 1 and or
    swap 1 rshift swap
  loop nip ;

: main
  0
  5000000 0 do
    i bitrev32 xor
  loop
  . cr ;
