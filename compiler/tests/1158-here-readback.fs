\ expect: 5 4 3 2 1
create tbl 40 allot
: main
  5 0 do 5 i - tbl i cells + ! loop
  5 0 do tbl i cells + @ . loop cr ;
