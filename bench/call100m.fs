\ BENCH compile=10 run=135
\ 100M calls to a 1-word function - measures call/ret overhead
: inc1 ( n -- n ) 1+ ;
: main 0 100000000 0 do inc1 loop . cr ;
