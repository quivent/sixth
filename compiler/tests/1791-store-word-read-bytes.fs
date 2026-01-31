\ expect: 255 0
create buf 8 allot
: main
  255 buf !
  buf c@ . buf 1 + c@ . ;
