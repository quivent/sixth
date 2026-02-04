\ expected: 49995000
\ Sum array - pattern GCC can auto-vectorize

create arr 80000 allot

: init ( -- ) 10000 0 do i i 8 * arr + ! loop ;
: sum-arr ( -- n ) 0 10000 0 do i 8 * arr + @ + loop ;
: main init sum-arr . cr ;
