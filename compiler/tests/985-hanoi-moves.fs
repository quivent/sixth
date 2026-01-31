\ expect: 15
\ Test 985: Tower of Hanoi move count for n disks = 2^n - 1, n=4 -> 15
: hanoi-count 1 swap 0 do 2* loop 1- ;
: main 4 hanoi-count . cr ;
