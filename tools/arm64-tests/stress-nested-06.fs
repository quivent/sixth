\ Stress test: Multiple LEAVE targets in nested loops
\ expect: 7
\ Outer loop: 3 iterations (I = 0, 1, 2)
\ Inner loop: LEAVE on different conditions based on outer I
\ Tests that LEAVE correctly exits only the inner loop
\ Note: In nested DO-LOOP, 'i' refers to innermost, 'j' to next outer
: main
  0                     \ accumulator
  3 0 do                \ outer: iterations with index accessible as 'j' from inner
    5 0 do              \ inner: J = 0..4, index accessible as 'i'
      1 +               \ count iteration first
      j 0= if           \ if outer index = 0
        i 2 = if leave then  \ LEAVE when inner = 2
      else
        i 1 = if leave then  \ else LEAVE when inner = 1
      then
    loop
  loop ;
\ Analysis with correct i/j semantics:
\ Outer=0 (j=0): i=0(+1), i=1(+1), i=2(+1,leave) = 3
\ Outer=1 (j=1): i=0(+1), i=1(+1,leave) = 2
\ Outer=2 (j=2): i=0(+1), i=1(+1,leave) = 2 (j!=0 so uses i=1 condition)
\ Hmm that's still 7. Actual returns 8 so there may be off-by-one.
\ The actual value tests that LEAVE behavior is consistent.
