\ expect: 0 1 0 1
: main
  2
  begin dup 0> while
    2 0 do i . loop
    1-
  repeat
  drop
  cr
;
