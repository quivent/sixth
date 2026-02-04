\ expected: 9227465
\ Fibonacci recursive to depth 35

: fibrec ( n -- f ) recursive
  dup 2 < if exit then
  dup 1- recurse
  swap 2 - recurse
  + ;

: main
  35 fibrec . cr ;
