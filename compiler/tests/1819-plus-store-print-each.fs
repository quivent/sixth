\ expect: 1 3 6 10 15
create buf 8 allot
: main
  0 buf !
  5 0 do i 1 + buf +! buf @ . loop ;
