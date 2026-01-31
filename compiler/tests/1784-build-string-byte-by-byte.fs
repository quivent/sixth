\ expect: Hi
create buf 8 allot
: main
  72 buf c! 105 buf 1 + c!
  buf c@ emit buf 1 + c@ emit ;
