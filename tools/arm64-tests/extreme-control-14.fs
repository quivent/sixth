\ expect: 77
\ Test: Multiple sequential IF-ELSE with correct patch target isolation
\ Each IF-ELSE is independent; patches must not cross-contaminate
\ Tests: gen-then patch isolation, cf-stack cleanup between constructs

: branch1 ( n -- n )
  dup 10 < if
    drop 10
  else
    drop 20
  then
;

: branch2 ( n -- n )
  dup 5 = if
    drop 50
  else
    dup 6 = if
      drop 60
    else
      dup 7 = if
        drop 70
      else
        drop 99
      then
    then
  then
;

: branch3 ( n -- n )
  \ Alternating true/false to exercise both paths
  1 if
    0 if
      100
    else
      1 if
        0 if
          200
        else
          7   \ This path taken
        then
      else
        300
      then
    then
  else
    400
  then
;

: main
  5 branch1         \ 5 < 10, returns 10
  7 branch2         \ 7 = 7, returns 70
  branch3           \ returns 7 (from nested else path)
  + + 10 -          \ 10 + 70 + 7 - 10 = 77
;
