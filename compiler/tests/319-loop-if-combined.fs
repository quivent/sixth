\ expect: 30
\ Test: loop with conditional accumulation → 25
: main 0 10 begin dup 0 > while dup 2 mod 0= if swap over + swap then 1- repeat drop . cr ;
