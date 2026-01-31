\ expect: 5 10 15 20
\ cell+ combined with @ for sequential reads
create arr 32 allot
: main
  5 arr !
  10 arr 8 + !
  15 arr 16 + !
  20 arr 24 + !
  arr dup @ . cell+
  dup @ . cell+
  dup @ . cell+
  @ . cr ;
