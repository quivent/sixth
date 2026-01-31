\ expect: 10
create cnt 8 allot
: main
  0 cnt !
  begin 1 cnt +! cnt @ 10 = until
  cnt @ . ;
