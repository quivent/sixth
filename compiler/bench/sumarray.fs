\ expected: 49995000
\ Sum and product array benchmark - sum 10000 elements, run 1000 times

10000 constant SIZE
create arr SIZE cells allot

: init-arr ( -- )
  SIZE 0 do i i cells arr + ! loop ;

: sum-arr ( -- sum )
  0 SIZE 0 do i cells arr + @ + loop ;

: main ( -- )
  init-arr
  0
  1000 0 do
    drop sum-arr
  loop
  . cr ;
