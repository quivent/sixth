\ Adversarial Stack Test 15: Stress test
\ expect: 0
\ Combined stress test for stack operations

: t-push-pop ( -- flag )
  \ Push 20 items, verify TOS and bottom
  1 2 3 4 5 6 7 8 9 10
  11 12 13 14 15 16 17 18 19 20
  \ Stack: 1..20 (20 items)
  20 = if
    \ `=` consumed 20 and literal 20, pushed flag. `if` consumed flag.
    \ Stack: 1..19 (19 items)
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop
    \ Stack: 1 (1 item)
    1 = if 0 else 1 then
  else
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop
    1
  then ;

: t-ops ( -- flag )
  \ Complex sequence - test stack ops work without crash
  1 2 3 4 5
  swap rot over nip tuck
  \ Trace: 1 2 3 4 5
  \   swap: 1 2 3 5 4
  \   rot:  1 2 5 4 3
  \   over: 1 2 5 4 3 4  (6 items)
  \   nip:  1 2 5 4 4
  \   tuck: 1 2 5 4 4 4  (6 items)
  drop drop drop drop drop drop
  0 ;

: t-loop-ops ( -- flag )
  0
  10 0 do
    i 1+ +
  loop
  55 = if 0 else 1 then ;

: s-inc ( n -- n ) 1+ ;

: t-chain ( -- flag )
  1
  s-inc s-inc s-inc s-inc s-inc
  s-inc s-inc s-inc s-inc s-inc
  11 = if 0 else 1 then ;

: t-deep ( -- flag )
  \ Push 30 items, verify TOS
  1 2 3 4 5 6 7 8 9 10
  11 12 13 14 15 16 17 18 19 20
  21 22 23 24 25 26 27 28 29 30
  \ Stack: 1..30 (30 items)
  30 = if
    \ Stack now: 1..29 (29 items after = and if)
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop
    0
  else
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    drop drop drop drop drop
    1
  then ;

: main
  t-push-pop
  t-ops +
  t-loop-ops +
  t-chain +
  t-deep + ;
