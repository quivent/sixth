\ expected: 0
\ CRC32 checksum - compute CRC32 over many iterations

256 constant TABLE-SIZE
create crc-table TABLE-SIZE cells allot

: init-table ( -- )
  TABLE-SIZE 0 do
    i
    8 0 do
      dup 1 and if
        1 rshift $EDB88320 xor
      else
        1 rshift
      then
    loop
    crc-table i cells + !
  loop ;

: crc32-byte ( crc byte -- crc' )
  over xor $FF and cells crc-table + @
  swap 8 rshift xor ;

: crc32 ( addr len -- crc )
  $FFFFFFFF rot rot
  0 do
    over i + c@ crc32-byte
  loop
  nip $FFFFFFFF xor ;

create test-data 256 allot
: init-data ( -- )
  256 0 do i test-data i + c! loop ;

: main
  init-table
  init-data
  0
  10000 0 do
    test-data 256 crc32
    xor
  loop
  . cr ;
