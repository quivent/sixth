\ expected: 6564120420
\ Catalan number recursively, n=20

: catalan ( n -- c ) recursive
  dup 1 <= if drop 1 exit then
  0 over 0 do
    over 1- i - recurse
    i recurse
    * +
  loop nip ;

: main
  20 catalan . cr ;
