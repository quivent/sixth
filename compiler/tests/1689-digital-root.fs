\ expect: 6
\ Digital root of 12345: 1+2+3+4+5=15, 1+5=6
: digitsum ( n -- sum )
  0 swap
  begin dup 0> while
    dup 10 mod rot + swap
    10 /
  repeat drop ;
: droot ( n -- root )
  begin dup 9 > while digitsum repeat ;
: main 12345 droot . cr ;
