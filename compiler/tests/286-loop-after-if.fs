\ expect: 42
\ Test: loop inside if branch → 42
: main 1 if 3 begin 1- dup 0= until drop then 42 . cr ;
