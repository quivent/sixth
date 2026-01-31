\ expect: 10 20 30
create buf 24 allot
: main
  0 buf ! 0 buf 8 + ! 0 buf 16 + !
  10 buf +! 20 buf 8 + +! 30 buf 16 + +!
  buf @ . buf 8 + @ . buf 16 + @ . ;
