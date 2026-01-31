\ expect: 24
variable result
: fact ( n -- ) dup 1 > if dup result @ * result ! 1- recurse else drop then ;
: main 1 result ! 4 fact result @ . cr ;
