\ expect: 7
\ Test: BEGIN-UNTIL with nested BEGIN-WHILE-REPEAT
\ Two different loop terminators interleaved

: main
  0                     \ outer counter
  begin
    1+                  \ increment outer

    3                   \ inner start
    begin
      dup 0>
    while
      1-
    repeat
    drop

    dup 7 >=
  until
;
