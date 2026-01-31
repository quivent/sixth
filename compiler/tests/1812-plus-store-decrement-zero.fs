\ expect: 0
create buf 8 allot
: main
  5 buf ! -1 buf +! -1 buf +! -1 buf +! -1 buf +! -1 buf +!
  buf @ . ;
