\ expected: 99994
\ AVL insert 50K nodes, sum of final heights

50000 constant N
create left N cells allot
create right N cells allot
create ht N cells allot
create key N cells allot
variable root
variable ncount

: height ( node -- h ) dup 0< if drop 0 else cells ht + @ then ;
: max ( a b -- max ) 2dup < if swap then drop ;
: update-ht ( node -- ) dup cells left + @ height over cells right + @ height max 1+ swap cells ht + ! ;
: balance ( node -- bf ) dup cells left + @ height swap cells right + @ height - ;

: rotate-right ( y -- x )
  dup cells left + @    \ x = y.left
  dup cells right + @   \ x.right
  2 pick cells left + ! \ y.left = x.right
  over update-ht
  swap cells right + !  \ x.right = y
  dup update-ht ;

: rotate-left ( x -- y )
  dup cells right + @   \ y = x.right
  dup cells left + @    \ y.left
  2 pick cells right + !
  over update-ht
  swap cells left + !
  dup update-ht ;

: rebalance ( node -- node' )
  dup balance 1 > if
    dup cells left + @ balance 0< if
      dup cells left + @ rotate-left over cells left + !
    then
    rotate-right exit
  then
  dup balance -1 < if
    dup cells right + @ balance 0> if
      dup cells right + @ rotate-right over cells right + !
    then
    rotate-left exit
  then
  dup update-ht ;

: avl-insert ( val node -- node' )
  dup 0< if
    drop
    ncount @
    over ncount @ cells key + !
    -1 ncount @ cells left + !
    -1 ncount @ cells right + !
    1 ncount @ cells ht + !
    1 ncount +!
    nip exit
  then
  over over cells key + @ < if
    2dup cells left + @ recurse
    swap cells left + !
    nip rebalance
  else
    2dup cells right + @ recurse
    swap cells right + !
    nip rebalance
  then ;

: lcg ( n -- n' ) 1103515245 * 12345 + 2147483647 and ;

: build-tree ( -- )
  -1 root ! 0 ncount !
  12345
  N 0 do
    lcg dup N mod root @ avl-insert root !
  loop
  drop ;

: sum-heights ( -- n )
  0 N 0 do i cells ht + @ + loop ;

: main build-tree sum-heights . cr ;
