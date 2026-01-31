\ expect: 5 4 3 2 1
: countdown dup 0 > if dup . 1- countdown then ;
: main 5 countdown cr ;
