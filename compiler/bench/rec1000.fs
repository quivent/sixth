\ expected: 100000
\ Recursion depth 1000 per call, 100 outer iterations

: rec1000-helper ( n -- n ) recursive
  dup 0= if exit then
  1- recurse 1+ ;

: main
  0 100 0 do
    1000 rec1000-helper +
  loop . cr ;
