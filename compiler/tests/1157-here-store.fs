\ expect: 55
create tbl 40 allot
: main
  10 0 do i 1+ tbl i cells + ! loop
  0 10 0 do tbl i cells + @ + loop . cr ;
