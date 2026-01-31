\ expect: 17
create buf 8 allot
: main
  0 buf ! 3 4 + buf +! 2 5 * buf +!
  buf @ . ;
