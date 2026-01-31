\ expect: 99 0
create buf 8 allot
: main
  0 buf ! 99 buf c!
  buf c@ . buf 1 + c@ . ;
