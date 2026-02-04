\ expected: 499016000
\ Partition around pivot benchmark - 1M iterations

1000 constant SIZE
1000000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@
  rot arr! swap arr! ;

: do-partition ( pivot -- count-less )
  0 SIZE 1-               \ pivot i j
  begin 2dup < while
    begin 2dup < 3 pick 3 pick arr@ > and while
      1 under+
    repeat
    begin 2dup < 3 pick 2 pick arr@ <= and while
      1-
    repeat
    2dup < if 2dup swap-arr 1 under+ 1- then
  repeat
  nip nip ;

: init-arr ( -- )
  SIZE 0 do
    i 17 * 23 + SIZE mod i arr!
  loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    init-arr
    i SIZE mod do-partition +
  loop ;

: main
  bench . cr ;
