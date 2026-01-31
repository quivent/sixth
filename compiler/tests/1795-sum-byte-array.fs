\ expect: 15
create buf 8 allot
: main
  1 buf c! 2 buf 1 + c! 3 buf 2 + c! 4 buf 3 + c! 5 buf 4 + c!
  0 5 0 do buf i + c@ + loop . ;
