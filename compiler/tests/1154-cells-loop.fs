\ expect: 0 10 20 30 40
create arr 40 allot
: main
  5 0 do i 10 * arr i cells + ! loop
  5 0 do arr i cells + @ . loop cr ;
