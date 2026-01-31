\ BENCH compile=10 run=280
\ 50M iterations of mixed arithmetic - measures ALU pipeline
: step ( n -- n ) dup 3 * 7 + 2 / 5 mod 11 + ;
: main 0 50000000 0 do step loop . cr ;
