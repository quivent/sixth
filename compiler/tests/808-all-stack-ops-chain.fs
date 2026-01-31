\ Test 808: chain all stack ops and verify
\ 1 2 3: dup(1 2 3 3) rot(1 3 3 2) over(1 3 3 2 3) nip(1 3 3 3) 2drop(1 3) swap(3 1) +(4)
: main 1 2 3 dup rot over nip 2drop swap + . cr ;
