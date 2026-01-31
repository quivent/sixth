\ expect: 28
\ Sum of proper divisors of 28 (perfect number): 1+2+4+7+14 = 28
: sumdiv ( n -- sum )
  dup 1
  swap 2/ 1+ 2 do      \ check 2 to n/2
    over i mod 0= if
      i +
    then
  loop nip ;
: main 28 sumdiv . cr ;
