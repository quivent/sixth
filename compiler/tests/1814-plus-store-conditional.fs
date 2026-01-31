\ expect: 3
create buf 8 allot
: main
  0 buf !
  5 0 do i 2 mod 0= if 1 buf +! then loop
  buf @ . ;
