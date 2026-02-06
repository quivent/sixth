\ adversarial-loop-14-max-int.fs - Loop with limit near MAX-INT
\ 3 iterations near max-int boundary
\ expect: 3

: main
  0
  9223372036854775807 9223372036854775804 do
    1+
  loop
;
