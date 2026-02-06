\ expect: 1
\ Triple mutual recursion: A->B->C->A until zero
\ Tests that forward references work in a cycle
: ping dup 0= if drop 1 else 1- pong then ;
: pong dup 0= if drop 0 else 1- pang then ;
: pang dup 0= if drop 0 else 1- ping then ;
: main 9 ping ;
