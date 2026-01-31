\ expect: 30
\ Test 378: sum even numbers 1-10
: main 0 10 begin dup 0 > while dup 2 mod 0= if dup rot + swap then 1- repeat drop . cr ;
