\ expect: 100
create buf 8 allot
: main
  0 buf ! 100 buf +! buf @ . ;
