\ expect: 55
\ Tail-call with deep recursion: sum 1 to 10
\ Result fits in exit code range (0-255)
: sum-tail
  over 0= if nip exit then
  over + swap 1- swap sum-tail ;
: main 10 0 sum-tail ;
