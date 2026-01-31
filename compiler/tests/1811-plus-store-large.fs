\ expect: 1000000
create buf 8 allot
: main
  0 buf ! 1000000 buf +! buf @ . ;
