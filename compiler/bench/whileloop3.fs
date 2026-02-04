\ expected: 1000000000
\ Nested WHILE 2 deep, 100K x 10K
: main
  0 0
  begin
    dup 100000 <
  while
    0
    begin
      dup 10000 <
    while
      rot 1+ -rot
      1+
    repeat
    drop
    1+
  repeat
  drop . cr
;
