\ Adversarial control flow: 4-level nested if/else with mixed paths
\ Tests deepest nesting with alternating true/false conditions
\ expect: 13
: main
  1 if
    0 if
      1 if 10 else 11 then
    else
      1 if 13 else 14 then
    then
  else
    0 if 20 else 21 then
  then ;
