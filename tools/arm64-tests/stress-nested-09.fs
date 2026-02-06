\ Stress test: Loop with multiple EXIT points in called word
\ expect: 9
\ Word with two EXIT paths tested from a loop
\ The word returns different values based on input
\ This tests that EXIT properly returns from the helper, not the loop
: classify ( n -- m )
  dup 2 < if drop 0 exit then   \ EXIT path 1: small -> 0
  dup 4 > if drop 0 exit then   \ EXIT path 2: large -> 0
  ;                             \ fall through: return n itself

: main
  0                     \ accumulator
  6 0 do                \ I = 0, 1, 2, 3, 4, 5
    i classify +        \ add classification result
  loop ;
\ I=0: 0<2 -> EXIT 0
\ I=1: 1<2 -> EXIT 0
\ I=2: 2<2 false, 2>4 false -> return 2
\ I=3: 3<2 false, 3>4 false -> return 3
\ I=4: 4<2 false, 4>4 false -> return 4
\ I=5: 5<2 false, 5>4 true -> EXIT 0
\ Sum = 0+0+2+3+4+0 = 9
