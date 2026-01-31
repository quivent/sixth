\ expect: 1 2 3 4 5
create buf 8 allot
: main
  5 0 do i 1 + buf i + c! loop
  5 0 do buf i + c@ . loop ;
