\ expect: 55
\ 1-nzloop with accumulation: sum 10+9+8+...+1 = 55
\ Stack: ( sum counter ) with TOS=counter for 1-nzloop
: main 0 10 begin swap over + swap 1-nzloop drop . cr ;
