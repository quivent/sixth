\ expect: 1 2 3 4 5
create cnt 8 allot
: main
  0 cnt !
  5 0 do 1 cnt +! cnt @ . loop ;
