\ expect: 5 8
create buf 8 allot
: main
  0 buf ! 5 buf +! buf @ .
  3 buf +! buf @ . ;
