\ expect: L3 L2 L1
: go ( n -- )
  dup 0> if
    ." L" dup .
    1- go
  else drop then ;
: main 3 go cr ;
