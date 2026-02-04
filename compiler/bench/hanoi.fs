\ expected: 1048575
\ Towers of Hanoi, 20 disks - count moves

variable move-count

: hanoi ( n from to aux -- ) recursive
  over >r >r >r
  dup 0= if r> r> r> 2drop 2drop exit then
  dup 1- r@ r> swap r@ hanoi
  1 move-count +!
  r> r> 1- -rot hanoi ;

: main
  0 move-count !
  20 1 3 2 hanoi
  move-count @ . cr ;
