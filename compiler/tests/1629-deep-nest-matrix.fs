\ expect: 36
\ Sum j*3+i for j=0..2, i=0..2
\ 0+1+2+3+4+5+6+7+8 = 36
variable acc
: main
  0 acc !
  3 0 do
    3 0 do
      j 3 * i + acc @ + acc !
    loop
  loop
  acc @ . cr ;
