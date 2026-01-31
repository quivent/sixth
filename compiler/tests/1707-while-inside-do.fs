\ expect: 3 2 1 3 2 1 3 2 1
: main
  3 0 do
    3
    begin dup 0> while
      dup .
      1-
    repeat
    drop
  loop
  cr
;
