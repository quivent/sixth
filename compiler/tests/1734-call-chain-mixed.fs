\ expect: 15
: sum-to ( n -- sum )
  0 swap
  1+ 1 do i + loop
;
: double ( n -- n ) 2* ;
: half ( n -- n ) 2/ ;
: main
  5 sum-to double half .
  cr
;
