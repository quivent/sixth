\ expect: 55
variable acc
: addloop ( n -- ) recursive dup 0= if drop else dup acc @ + acc ! 1- recurse then ;
: main 0 acc ! 2 5 * addloop acc @ . cr ;
