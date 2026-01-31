\ expect: positive
: check ( n -- ) 0 > if ." positive" else ." negative" then ;
: main 5 check cr ;
