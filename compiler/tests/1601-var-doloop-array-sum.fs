\ expect: 150
create arr 40 allot
: main
  10 arr 0 cells + !
  20 arr 1 cells + !
  30 arr 2 cells + !
  40 arr 3 cells + !
  50 arr 4 cells + !
  0
  5 0 do arr i cells + @ + loop
  . cr ;
