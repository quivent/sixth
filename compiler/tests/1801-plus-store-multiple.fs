\ expect: 60
create buf 8 allot
: main
  0 buf ! 10 buf +! 20 buf +! 30 buf +!
  buf @ . ;
