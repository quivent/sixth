\ expect: 1 2 3 1 2 3
: main
  2
  begin
    1
    begin
      dup .
      1+
    dup 4 = until
    drop
    1-
  dup 0= until
  drop
  cr
;
