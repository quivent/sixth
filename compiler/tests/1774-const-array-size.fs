\ expect: 0 10 20 30 40
constant SZ 5
create arr 40 allot
: main SZ 0 do i 10 * arr i cells + ! loop SZ 0 do arr i cells + @ . loop cr ;
