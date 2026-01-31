\ expect: 0 1 2 0 1 2
: main
  2
  begin dup 0> while
    3 0 do i . loop
    1-
  repeat
  drop
  cr
;
