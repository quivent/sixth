\ expected: 102334155

: fib ( n -- f )
  dup 2 < if exit then
  dup 1 - recurse
  swap 2 - recurse
  + ;

: main ( -- )
  40 fib . cr ;
