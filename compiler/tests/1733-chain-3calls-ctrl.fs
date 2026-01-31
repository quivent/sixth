\ expect: 6 6 3
: double-if-pos ( n -- n )
  dup 0> if 2* then
;
: sum-loop ( n -- sum )
  0 swap
  1+ 1 do i + loop
;
: abs-val ( n -- n )
  dup 0< if negate then
;
: main
  3 double-if-pos .
  3 sum-loop .
  -3 abs-val .
  cr
;
