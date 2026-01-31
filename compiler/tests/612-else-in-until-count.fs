\ Test 612: only else branch increments accumulator
: main 0 1 begin dup 5 > if swap swap else swap 1+ swap then 1+ dup 8 > until drop . cr ;
