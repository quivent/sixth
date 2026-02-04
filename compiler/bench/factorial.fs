\ expected: 479001600
\ Factorial benchmark - compute factorial of 12, 100000 times

: factorial ( n -- n! )
  1 swap 1+ 1 ?do i * loop ;

: main ( -- )
  0
  100000 0 do
    drop 12 factorial
  loop
  . cr ;
