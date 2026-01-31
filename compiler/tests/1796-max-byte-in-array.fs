\ expect: 50
create buf 8 allot
: main
  10 buf c! 50 buf 1 + c! 30 buf 2 + c! 20 buf 3 + c!
  0 4 0 do buf i + c@ max loop . ;
