\ Adversarial: Test 0>= and 0<= in loop conditions
\ expect: 1
: count-down ( n -- count )
  0 swap           \ count n
  begin
    dup 0>=        \ while n >= 0
  while
    swap 1+ swap   \ count++
    1-             \ n--
  repeat
  drop ;           \ drop n, leave count

: count-up ( n -- count )
  0 swap           \ count n
  begin
    dup 0<=        \ while n <= 0
  while
    swap 1+ swap   \ count++
    1+             \ n++
  repeat
  drop ;           \ drop n, leave count

: main
  5 count-down     \ starting from 5, counts: 5,4,3,2,1,0 = 6 iterations
  6 =
  -5 count-up      \ starting from -5, counts: -5,-4,-3,-2,-1,0 = 6 iterations
  6 =
  and
  if 1 else 0 then ;
