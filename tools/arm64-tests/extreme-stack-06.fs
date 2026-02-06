\ expect: 77
\ Test: Stack ops inside BEGIN-WHILE-REPEAT
: main
  0
  10
  begin
    dup 0>
  while
    swap 1 2 3 4 5 6 7
    + + + + + + +
    swap 1-
  repeat
  drop
  280 - 77 +
;
