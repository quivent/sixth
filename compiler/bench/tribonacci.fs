\ expected: 989640352
\ Tribonacci sequence - each term is sum of previous three

: tribonacci ( n -- t_n )
  dup 0= if exit then
  dup 1 = if drop 0 exit then
  dup 2 = if drop 1 exit then
  0 0 1                   \ a=0, b=0, c=1 (t0, t1, t2)
  4 pick 2 do             \ loop n-2 times
    2dup + 3 pick +       \ a b c (a+b+c)
    >r rot drop r>        \ b c (a+b+c) -> becomes new a b c
  loop
  nip nip nip ;

: main
  0
  5000000 0 do
    i 37 mod tribonacci xor
  loop
  . cr ;
