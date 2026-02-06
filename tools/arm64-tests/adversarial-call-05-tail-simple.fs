\ expect: 100
\ Tail-call scenario: accumulate via tail position
\ Each call either returns or makes a tail call
: accum ( n acc -- acc )
  over 0= if nip exit then
  swap 1- swap 1+ accum ;
: main 100 0 accum ;
