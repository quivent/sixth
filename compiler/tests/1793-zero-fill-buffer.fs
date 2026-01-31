\ expect: 0 0 0 0 0
create buf 8 allot
: main
  99 buf c! 99 buf 1 + c! 99 buf 2 + c! 99 buf 3 + c! 99 buf 4 + c!
  5 0 do 0 buf i + c! loop
  5 0 do buf i + c@ . loop ;
