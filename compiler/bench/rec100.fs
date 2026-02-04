\ expected: 1000000
\ Recursion depth 100 per call, 10K outer iterations

: rec100-helper ( n -- n ) recursive
  dup 0= if exit then
  1- recurse 1+ ;

: main
  0 10000 0 do
    100 rec100-helper +
  loop . cr ;
