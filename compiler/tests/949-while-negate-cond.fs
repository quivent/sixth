\ expect: 0
\ Test 949: while with negate as condition modifier
\ -3 -> negate -> 3 (true), loop body increments toward 0
: main -3 begin dup negate while 1+ repeat . cr ;
