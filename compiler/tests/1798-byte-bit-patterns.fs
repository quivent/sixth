\ expect: 255 0 128
create buf 8 allot
: main
  255 buf c! 0 buf 1 + c! 128 buf 2 + c!
  buf c@ . buf 1 + c@ . buf 2 + c@ . ;
