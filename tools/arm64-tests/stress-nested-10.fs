\ Stress test: Word with internal loop called from another loop
\ expect: 15
\ Inner word: has a DO-LOOP that iterates based on parameter
\ Outer: calls inner word multiple times
\ Tests that loop state is properly saved/restored across calls
variable accum

: add-n ( n -- )
  dup 0 ?do             \ loop n times (skip if n=0)
    accum @ 1 + accum !  \ increment accum each iteration
  loop
  drop ;                \ drop n

: main
  0 accum !             \ initialize accumulator
  6 1 do                \ I = 1, 2, 3, 4, 5
    i add-n             \ add-n loops I times
  loop
  accum @ ;             \ 1+2+3+4+5 = 15
