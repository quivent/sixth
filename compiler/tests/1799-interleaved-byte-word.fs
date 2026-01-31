\ expect: 1000 77
create buf 16 allot
: main
  1000 buf !
  77 buf 8 + c!
  buf @ . buf 8 + c@ . ;
