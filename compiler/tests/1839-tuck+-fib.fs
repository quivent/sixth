\ expect: 55
\ tuck+ computes Fibonacci step: ( a b -- b a+b )
\ Start (0 1), apply tuck+ 10 times for F(10)
\ (NOS=0,TOS=1) -> tuck+ -> (1,1) -> (1,2) -> (2,3) -> (3,5)
\ -> (5,8) -> (8,13) -> (13,21) -> (21,34) -> (34,55) -> (55,89)
\ 10 iterations. NOS=55, TOS=89. drop TOS, NOS=55. . prints 55.
: main 0 1 10 0 do tuck+ loop drop . cr ;
