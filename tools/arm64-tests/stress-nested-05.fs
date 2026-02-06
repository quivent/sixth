\ Stress test: LEAVE from inner loop only
\ expect: 15
\ Outer: 5 iterations (I = 0..4)
\ Inner: should run until J = I, then LEAVE, accumulate J values
\ I=0: inner J=0, 0=0 so LEAVE immediately with J=0 added => +0
\ I=1: J=0(add 0, not equal), J=1(add 1, equal LEAVE) => +1
\ I=2: J=0,1(add, not equal), J=2(add 2, LEAVE) => +0+1+2 = 3... wait
\ Hmm, the order matters. Let me think: add J, then check LEAVE
\ I=0: add 0, check 0=0 LEAVE => +0
\ I=1: add 0, check 0=1 no; add 1, check 1=1 LEAVE => +1
\ I=2: add 0,1, add 2, LEAVE => +3
\ I=3: add 0,1,2,3 LEAVE => +6
\ I=4: add 0,1,2,3,4 LEAVE => +10
\ Total = 0+1+3+6+10 = 20... let me simplify
\ Actually let's just count how many inner iterations run
\ I=0: 1 iter, I=1: 2 iter, I=2: 3 iter, I=3: 4 iter, I=4: 5 iter
\ Total iterations = 1+2+3+4+5 = 15
: main
  0                     \ accumulator
  5 0 do                \ I = 0, 1, 2, 3, 4
    10 0 do             \ J = 0..9, but we'll LEAVE early
      1 +               \ count iteration
      j i = if leave then  \ LEAVE when J = I (inner index = outer index)
    loop
  loop ;
