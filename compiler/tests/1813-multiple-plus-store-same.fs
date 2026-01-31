\ expect: 15
create buf 8 allot
: main
  0 buf !
  1 buf +! 2 buf +! 3 buf +! 4 buf +! 5 buf +!
  buf @ . ;
