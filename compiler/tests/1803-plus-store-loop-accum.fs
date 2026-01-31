\ expect: 10
create buf 8 allot
: main
  0 buf ! 5 0 do 2 buf +! loop buf @ . ;
