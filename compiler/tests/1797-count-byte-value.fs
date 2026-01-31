\ expect: 3
create buf 8 allot
: main
  7 buf c! 5 buf 1 + c! 7 buf 2 + c! 3 buf 3 + c! 7 buf 4 + c!
  0 5 0 do buf i + c@ 7 = if 1 + then loop . ;
