\ expect: 15
\ Towers of Hanoi: count moves for n disks
: hanoi ( n -- moves )
  dup 1 <= if drop 1 exit then
  1- dup hanoi     \ moves for n-1
  swap hanoi       \ moves for n-1 again
  + 1+ ;           \ 2*hanoi(n-1) + 1
: main 4 hanoi . cr ;
