\ expect: 54321
: reverse ( n -- rev )
  0 swap
  begin dup 0> while
    swap 10 * over 10 mod + swap
    10 /
  repeat drop ;
: main 12345 reverse . cr ;
