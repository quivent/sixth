\ expect: 5 3 2 1
\ Test 924: nip with 5 values (removes NOS)
\ Stack: 1 2 3 4 5 -> nip -> 1 2 3 5
: main 1 2 3 4 5 nip . . . . cr ;
