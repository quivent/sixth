\ expect: 15 50 -5
: op-add ( a b -- c ) + ;
: op-mul ( a b -- c ) * ;
: op-sub ( a b -- c ) - ;
variable op
: dispatch ( a b -- c )
  op @ 0= if op-add else
  op @ 1 = if op-mul else
  op-sub
  then then ;
: main
  0 op ! 5 10 dispatch .
  1 op ! 5 10 dispatch .
  2 op ! 5 10 dispatch .
  cr ;
