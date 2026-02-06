\ expect: 0
\ Brutal Integration Test 10: Simple Hash Table
\ Tests: hashing, pointer chains, lookups

variable buckets-base
variable entries-base
variable entry-cnt

: hash ( key -- idx ) 7 and ;

: bucket@ ( idx -- ptr ) cells buckets-base @ + @ ;
: bucket! ( ptr idx -- ) cells buckets-base @ + ! ;

: entry-key ( e -- addr ) ;
: entry-val ( e -- addr ) cell+ ;
: entry-next ( e -- addr ) 2 cells + ;

: alloc-entry ( -- entry )
  entry-cnt @ 3 * cells entries-base @ +
  entry-cnt @ 1+ entry-cnt ! ;

: ht-init ( -- )
  here buckets-base ! 8 cells allot
  here entries-base ! 64 cells allot
  8 0 do 0 i bucket! loop
  0 entry-cnt ! ;

: ht-put ( key val -- )
  swap dup hash >r
  alloc-entry >r
  r@ entry-key !
  r@ entry-val !
  r> dup entry-next r@ bucket@ swap !
  r> bucket! ;

: ht-get ( key -- val found? )
  dup hash bucket@
  begin dup while
    2dup @ = if
      nip entry-val @ 1 exit
    then
    entry-next @
  repeat
  nip 0 ;

: main
  ht-init
  10 100 ht-put
  5 50 ht-put
  18 180 ht-put
  10 ht-get 0= if 1 exit then 100 <> if 1 exit then
  5 ht-get 0= if 1 exit then 50 <> if 1 exit then
  18 ht-get 0= if 1 exit then 180 <> if 1 exit then
  99 ht-get if drop 1 exit then drop
  0 ;
