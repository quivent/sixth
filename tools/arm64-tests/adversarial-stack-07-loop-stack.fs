\ Adversarial Stack Test 07: Stack operations after loops
\ expect: 0
\ Test that stack state is preserved correctly across loop iterations

: t-loop-push ( -- flag )
  0
  5 0 do i + loop
  \ Sum of 0+1+2+3+4 = 10
  10 = if 0 else 1 then ;

: t-nested-lp ( -- flag )
  0
  3 0 do
    3 0 do
      1+
    loop
  loop
  \ 3x3 = 9 iterations
  9 = if 0 else 1 then ;

: t-loop-dup ( -- flag )
  1
  4 0 do dup + loop
  \ 1 -> 2 -> 4 -> 8 -> 16
  16 = if 0 else 1 then ;

: t-loop-depth ( -- flag )
  1 2 3
  5 0 do
    rot
  loop
  \ After 5 rots: 1 2 3 (back to original due to 5 = 3+2 rots)
  \ Actually: rot 5x on 3 items cycles: 3->1->2->3->1->2
  2 = swap 1 = and swap 3 = and
  if 0 else 1 then ;

: main
  t-loop-push
  t-nested-lp +
  t-loop-dup +
  t-loop-depth + ;
