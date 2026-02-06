\ expect: 0
\ Brutal Integration Test 09: Linked List Insertion
\ Tests: pointer manipulation, dynamic allocation patterns

variable nodes-base
variable node-cnt
variable head-ptr

: node-val ( node -- addr ) ;
: node-next ( node -- addr ) cell+ ;

: alloc-node ( -- node )
  node-cnt @ 2 * cells nodes-base @ +
  node-cnt @ 1+ node-cnt ! ;

: make-node ( val -- node )
  alloc-node dup >r swap r@ ! r> ;

: insert-front ( val -- )
  make-node dup node-next head-ptr @ swap ! head-ptr ! ;

: list-len ( -- n )
  0 head-ptr @
  begin dup while
    swap 1+ swap node-next @
  repeat
  drop ;

: list-sum ( -- sum )
  0 head-ptr @
  begin dup while
    dup @ rot + swap node-next @
  repeat
  drop ;

: init-list ( -- )
  here nodes-base ! 100 cells allot
  0 node-cnt !
  0 head-ptr ! ;

: main
  init-list
  5 insert-front
  3 insert-front
  7 insert-front
  2 insert-front
  list-len 4 <> if 1 exit then
  list-sum 17 <> if 1 exit then
  head-ptr @ @ 2 <> if 1 exit then
  0 ;
