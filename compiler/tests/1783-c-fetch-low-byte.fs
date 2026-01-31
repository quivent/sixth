\ expect: 1 1
create buf 8 allot
: main
  257 buf ! buf c@ . buf 1 + c@ . ;
