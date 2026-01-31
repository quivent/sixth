\ expect: 5
variable calls
: count-down ( n -- )
  1 calls +!
  dup 1 > if 1- count-down else drop then ;
: main 0 calls ! 5 count-down calls @ . cr ;
