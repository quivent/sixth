\ expect: 1 2 3
create buf 8 allot
: main
  1 buf c! 2 buf 1+ c! 3 buf 2 + c!
  buf c@ . buf 1+ c@ . buf 2 + c@ . cr ;
