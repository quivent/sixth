\ expect: 2 1 2 1
: main
  2
  begin dup 0> while
    dup
    2
    begin dup 0> while
      dup .
      1-
    repeat
    drop
    drop
    1-
  repeat
  drop
  cr
;
