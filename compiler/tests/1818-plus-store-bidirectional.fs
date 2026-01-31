\ expect: 2
create buf 8 allot
: main
  0 buf !
  5 buf +! -3 buf +! 4 buf +! -2 buf +! -2 buf +!
  buf @ . ;
