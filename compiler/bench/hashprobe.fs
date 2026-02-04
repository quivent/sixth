\ expected: 499500000
\ Hash table linear probing benchmark - 1M lookups

2048 constant TSIZE
1000000 constant ITERS
create htbl TSIZE cells allot
create hval TSIZE cells allot

: htbl@ ( i -- n ) cells htbl + @ ;
: htbl! ( n i -- ) cells htbl + ! ;
: hval@ ( i -- n ) cells hval + @ ;
: hval! ( n i -- ) cells hval + ! ;

: hash ( key -- h )
  dup 5 lshift swap 27 rshift or
  dup xor
  TSIZE 1- and ;

: hash-insert ( val key -- )
  dup hash                \ val key h
  begin
    dup htbl@ -1 <>
  while
    1+ TSIZE 1- and       \ linear probe
  repeat
  tuck htbl!              \ val h ; key->htbl[h]
  hval! ;                 \ val->hval[h]

: hash-find ( key -- val|-1 )
  dup hash                \ key h
  begin
    dup htbl@ dup -1 <> rot 3 pick <> and
  while
    drop
    1+ TSIZE 1- and       \ linear probe
  repeat
  htbl@ -1 = if
    drop -1
  else
    hval@
  then ;

: init-htbl ( -- )
  TSIZE 0 do -1 i htbl! loop
  1000 0 do i i hash-insert loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i 1000 mod hash-find +
  loop ;

: main
  init-htbl
  bench . cr ;
