\ expect: 3
create buf 8 allot
: main
  0 buf c!
  buf c@ 1 + buf c! buf c@ 1 + buf c! buf c@ 1 + buf c!
  buf c@ . ;
