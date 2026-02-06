\ Adversarial test: Maximum stress - nested DO with IF
\ Tests deep nesting with DO-LOOP and IF
\ expect: 42

: helper ( n -- n' )
  dup 5 > if
    2 -       \ subtract 2 if > 5
  else
    1+        \ add 1 otherwise
  then
;

: main
  0           \ result accumulator
  3 0 do      \ outer DO (i=0,1,2)
    4 0 do    \ inner DO (j=0,1,2,3)
      i j + helper  \ apply helper to (i + j)
      +             \ add to accumulator
    loop
  loop
;
\ i=0: j=0,1,2,3 -> helper(0,1,2,3) = 1,2,3,4 -> sum=10
\ i=1: j=0,1,2,3 -> helper(1,2,3,4) = 2,3,4,5 -> sum=14
\ i=2: j=0,1,2,3 -> helper(2,3,4,5) = 3,4,5,6 -> sum=18
\ Total: 10 + 14 + 18 = 42
