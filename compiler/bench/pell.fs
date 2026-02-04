\ expected: 0
\ Pell numbers - P(n) = 2*P(n-1) + P(n-2)

: pell ( n -- P_n )
  dup 0= if exit then
  dup 1 = if exit then
  0 1                     \ a=P0=0, b=P1=1 (stack: n a b)
  rot 1 do                \ loop from 1 to n-1
    tuck 2 * +            \ (a b) -> (b a b) -> (b a 2b) -> (b 2b+a)
  loop nip ;

: main
  0
  5000000 0 do
    i 32 mod pell xor
  loop
  . cr ;
