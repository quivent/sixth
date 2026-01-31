: isqrt ( n -- root ) 0 begin 2dup dup * >= while 1+ repeat 1- nip ;
: main 144 isqrt . cr ;
