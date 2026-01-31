\ expect: ABCDE
create buf 8 allot
: main
  65 buf c! 66 buf 1 + c! 67 buf 2 + c! 68 buf 3 + c! 69 buf 4 + c!
  5 0 do buf i + c@ emit loop ;
