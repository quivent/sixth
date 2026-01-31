: fib ( n -- f ) 0 1 rot 0 do swap over + loop nip ;
: main 70 fib . cr ;
