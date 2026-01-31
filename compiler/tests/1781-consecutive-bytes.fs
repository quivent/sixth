\ expect: 10 20 30
create buf 8 allot
: main
  10 buf c! 20 buf 1 + c! 30 buf 2 + c!
  buf c@ . buf 1 + c@ . buf 2 + c@ . ;
