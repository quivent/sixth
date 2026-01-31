\ expect: 15
: digit-sum ( n -- sum )
  0 swap
  begin dup 0> while
    dup 10 mod rot + swap
    10 /
  repeat drop ;
: main 12345 digit-sum . cr ;
