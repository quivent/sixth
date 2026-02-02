\ expect: done
: go recursive dup 0 > if dup . 1- recurse else drop then ;
: main 0 go ." done" cr ;
