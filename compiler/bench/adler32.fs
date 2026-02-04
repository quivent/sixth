\ expected: 0
\ Adler32 checksum - compute adler32 over many iterations

65521 constant MOD-ADLER

create test-data 256 allot
: init-data ( -- )
  256 0 do i test-data i + c! loop ;

: adler32 ( addr len -- checksum )
  1 swap 0 swap   \ a=1, b=0
  0 do            \ ( addr a b )
    2 pick i + c@
    rot + MOD-ADLER mod
    swap over + MOD-ADLER mod
  loop
  16 lshift or nip ;

: main
  init-data
  0
  50000 0 do
    test-data 256 adler32
    xor
  loop
  . cr ;
