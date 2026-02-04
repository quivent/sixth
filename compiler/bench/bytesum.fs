\ expected: 32640
\ Byte sum checksum - compute sum of bytes over many iterations

create test-data 256 allot
: init-data ( -- )
  256 0 do i test-data i + c! loop ;

: bytesum ( addr len -- checksum )
  0 swap 0 do
    over i + c@ +
  loop nip ;

: main
  init-data
  0
  500000 0 do
    test-data 256 bytesum
    xor
  loop
  . cr ;
