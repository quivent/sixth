\ Adversarial control flow: while loop nested inside if
\ Tests begin/while/repeat inside conditional branch
\ expect: 10
: main
  1 if
    0 5 begin dup 0> while swap 2 + swap 1 - repeat drop
  else
    99
  then ;
