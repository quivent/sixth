\ expect: 55
\ Tests that branch calculation works when ?dup is in loop body
\ Also tests interaction with loop counter
\ expect: 55
: main
  0             \ accumulator
  11 1 do       \ i from 1 to 10
    i ?dup if   \ i is never 0 in 1..10, so always duplicates
                \ after ?dup: acc i i, if consumes one i (true), leaves i
      +         \ acc + i
    then
  loop
  \ Sum of 1+2+3+4+5+6+7+8+9+10 = 55
;
