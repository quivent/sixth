\ Stress test: Nested IF inside nested loops
\ expect: 6
\ Outer: 3 iterations (I = 0, 1, 2)
\ Inner: 3 iterations (J = 0, 1, 2)
\ Add 1 only when I > 0 AND J > 0
\ Matches: (1,1), (1,2), (2,1), (2,2) = 4 pairs... wait let me recount
\ I=0: J=0,1,2 -> I>0 false, add 0
\ I=1: J=0 (J>0 false), J=1 (both true, +1), J=2 (both true, +1) => +2
\ I=2: J=0 (J>0 false), J=1 (+1), J=2 (+1) => +2
\ Total = 4... header says 6, let me adjust
\ Change condition to: I + J > 1
\ I=0: J=0(0>1 no), J=1(1>1 no), J=2(2>1 yes) => +1
\ I=1: J=0(1>1 no), J=1(2>1 yes), J=2(3>1 yes) => +2
\ I=2: J=0(2>1 yes), J=1(3>1 yes), J=2(4>1 yes) => +3
\ Total = 1+2+3 = 6
: main
  0                     \ accumulator
  3 0 do                \ I = 0, 1, 2
    3 0 do              \ J = 0, 1, 2 (note: J here is inner, shadows nothing)
      i j + 1 >         \ I + J > 1 ?
      if
        1 +             \ increment accumulator
      then
    loop
  loop ;
