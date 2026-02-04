\ expected: 10000000
\ Recursion depth 10 per call, 1M outer iterations

: rec10-helper ( n -- n ) recursive
  dup 0= if exit then
  1- recurse 1+ ;

: main
  0 1000000 0 do
    10 rec10-helper +
  loop . cr ;
