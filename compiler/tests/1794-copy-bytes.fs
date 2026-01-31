\ expect: 11 22 33
create src 8 allot
create dst 8 allot
: main
  11 src c! 22 src 1 + c! 33 src 2 + c!
  3 0 do src i + c@ dst i + c! loop
  3 0 do dst i + c@ . loop ;
