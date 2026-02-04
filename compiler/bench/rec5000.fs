\ expected: 50000
\ Recursion depth 5000 per call, 10 outer iterations

: rec5000-helper ( n -- n ) recursive
  dup 0= if exit then
  1- recurse 1+ ;

: main
  0 10 0 do
    5000 rec5000-helper +
  loop . cr ;
