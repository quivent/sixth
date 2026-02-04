\ expected: 0
\ Fletcher16 checksum - compute fletcher16 over many iterations

create test-data 256 allot
: init-data ( -- )
  256 0 do i test-data i + c! loop ;

: fletcher16 ( addr len -- checksum )
  0 0 rot            \ sum1=0, sum2=0, len
  0 do               \ ( addr sum1 sum2 )
    2 pick i + c@
    rot + 255 mod
    swap over + 255 mod
  loop
  8 lshift or nip ;

: main
  init-data
  0
  100000 0 do
    test-data 256 fletcher16
    xor
  loop
  . cr ;
