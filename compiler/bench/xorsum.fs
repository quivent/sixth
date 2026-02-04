\ expected: 0
\ XOR checksum - compute xor checksum over many iterations

create test-data 256 allot
: init-data ( -- )
  256 0 do i test-data i + c! loop ;

: xorsum ( addr len -- checksum )
  0 swap 0 do
    over i + c@ xor
  loop nip ;

: main
  init-data
  0
  500000 0 do
    test-data 256 xorsum
    xor
  loop
  . cr ;
