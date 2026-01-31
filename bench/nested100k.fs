\ BENCH compile=10 run=410
\ 100k x 10k nested do/loop - measures loop overhead
: main 0 100000 0 do 10000 0 do 1+ loop loop . cr ;
