\ expect: 6
create buf 8 allot
: main
  0 buf !
  3 0 do 2 0 do 1 buf +! loop loop
  buf @ . ;
