\ expected: 1388710083
\ Lucas numbers - like Fibonacci but starts with 2, 1

: lucas ( n -- L_n )
  dup 0= if drop 2 exit then
  dup 1 = if drop 1 exit then
  2 1                     \ a=L0=2, b=L1=1 (stack: a b)
  rot 1 do
    tuck +                \ (a b) -> (b a b) -> (b a+b)
  loop nip ;

: main
  0
  5000000 0 do
    i 45 mod lucas xor
  loop
  . cr ;
