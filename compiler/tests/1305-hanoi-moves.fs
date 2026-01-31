\ expect: 15
: hanoi ( n -- moves )
  dup 0= if exit then
  1- dup hanoi 2 * 1+ ;
: main 4 hanoi . cr ;
