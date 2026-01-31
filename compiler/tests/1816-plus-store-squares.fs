\ expect: 30
create buf 8 allot
: main
  0 buf !
  5 1 do i i * buf +! loop
  buf @ . ;
