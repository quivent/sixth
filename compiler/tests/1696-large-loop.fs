\ expect: 333833500
\ Sum of i*i for i=1 to 1000: n(n+1)(2n+1)/6 = 333833500
: main
  0
  1001 1 do
    i i * +
  loop
  . cr ;
