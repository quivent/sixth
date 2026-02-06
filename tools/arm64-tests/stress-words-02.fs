\ stress-words-02.fs - Mutual recursion with countdown
\ expect: 1
\ A calls B, B calls A until n=0
\ Testing: is 5 odd? yes (1)
: ping dup 0= if drop 0 else 1- pong then ;
: pong dup 0= if drop 1 else 1- ping then ;
: main 5 ping ;
