\ expected: 49995000
\ Bucket sort benchmark - sort 10000 elements

10000 constant SIZE
100 constant NBUCKETS
create arr SIZE cells allot
create buckets NBUCKETS SIZE * cells allot
create bcount NBUCKETS cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;
: bcount@ ( i -- n ) cells bcount + @ ;
: bcount! ( n i -- ) cells bcount + ! ;
: bucket@ ( b i -- n ) swap SIZE * + cells buckets + @ ;
: bucket! ( n b i -- ) swap SIZE * + cells buckets + ! ;

: bucket-idx ( val -- b )
  SIZE / ;

: insert-bucket ( val -- )
  dup bucket-idx         \ val b
  dup bcount@            \ val b cnt
  2dup bucket!           \ store val at bucket[b][cnt]
  1+ swap bcount! ;      \ increment count

: sort-bucket ( b -- )
  \ Insertion sort on bucket b
  dup bcount@ 1 do       \ b
    dup i bucket@        \ b key
    i                    \ b key j
    begin
      dup 0> 2 pick 3 pick 2 pick 1- bucket@ < and
    while
      2 pick over 1- bucket@ 2 pick swap bucket!
      1-
    repeat
    2 pick swap bucket!  \ b
    drop
  loop drop ;

: collect-buckets ( -- )
  0                       \ idx
  NBUCKETS 1- 0 swap do   \ descending for descending order
    i bcount@ 0 do
      i j bucket@        \ idx val
      over arr!
      1+
    loop
  -1 +loop drop ;

: bucket-sort ( -- )
  NBUCKETS 0 do 0 i bcount! loop
  SIZE 0 do i arr@ insert-bucket loop
  NBUCKETS 0 do i sort-bucket loop
  collect-buckets ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  bucket-sort
  sum-arr . cr ;
