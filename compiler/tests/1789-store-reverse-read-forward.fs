\ expect: 3 2 1
create buf 8 allot
: main
  1 buf 2 + c! 2 buf 1 + c! 3 buf c!
  buf c@ . buf 1 + c@ . buf 2 + c@ . ;
