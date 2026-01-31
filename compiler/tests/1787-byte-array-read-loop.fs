\ expect: 10 20 30 40
create buf 8 allot
: main
  10 buf c! 20 buf 1 + c! 30 buf 2 + c! 40 buf 3 + c!
  4 0 do buf i + c@ . loop ;
