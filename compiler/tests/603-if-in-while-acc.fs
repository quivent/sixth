\ Test 603: if inside while - add only even counters
: main 0 1 begin dup 5 <= while dup 2 mod 0= if dup rot + swap then 1+ repeat drop . cr ;
