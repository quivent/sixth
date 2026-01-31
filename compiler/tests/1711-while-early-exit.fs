\ expect: 10 9 8 7 6
: main
  10
  begin dup 5 > while
    dup .
    1-
  repeat
  drop
  cr
;
