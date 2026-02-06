\ Adversarial control flow: if/else nested inside while loop
\ Tests control flow stack management with mixed structures
\ expect: 35
: main
  0 5
  begin dup 0> while
    dup 3 > if
      swap 10 + swap
    else
      swap 5 + swap
    then
    1 -
  repeat drop ;
