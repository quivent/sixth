\ expect: 15
\ nzloop with accumulation: sum 5+4+3+2+1
\ Stack: ( sum counter ) = ( 0 5 )
\ Each iteration: swap over + swap 1-
: main 0 5 begin swap over + swap 1- nzloop drop . cr ;
