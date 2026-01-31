\ expect: 10 10 20 20 30 30
create arr 24 allot
: main
  10 arr ! 20 arr 8 + ! 30 arr 16 + !
  3 0 do 2 0 do
    arr j cells + @ .
  loop loop cr ;
