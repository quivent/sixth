\ expect: 0 4 0 0
create buf 16 allot
: main
  1024 buf !
  buf c@ . buf 1 + c@ . buf 2 + c@ . buf 3 + c@ . ;
