: div3? ( n -- flag ) 3 mod 0= ;
: div5? ( n -- flag ) 5 mod 0= ;
: div7? ( n -- flag ) 7 mod 0= ;
: is-prime ( n -- 0|1 ) dup 2 < if drop 0 exit then dup 3 < if drop 1 exit then dup 2 mod 0= if drop 0 exit then dup 3 = if drop 1 exit then dup div3? if drop 0 exit then dup 5 = if drop 1 exit then dup div5? if drop 0 exit then dup 7 = if drop 1 exit then dup div7? if drop 0 exit then drop 1 ;
: main 0 51 2 do i is-prime + loop . cr ;
