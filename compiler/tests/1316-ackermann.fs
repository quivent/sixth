\ expect: 9
: ack1 ( n -- result ) dup 0= if drop 2 else 1- ack1 1+ then ;
: ack2 ( n -- result ) dup 0= if drop 1 ack1 else 1- ack2 ack1 then ;
: main 3 ack2 . cr ;
