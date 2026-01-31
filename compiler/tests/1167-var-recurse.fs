\ expect: 120
variable result
: fact ( n -- ) dup 1 > if dup result @ * result ! 1- fact else drop then ;
: main 1 result ! 5 fact result @ . cr ;
