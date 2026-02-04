\ expected: 774920
\ Reverse 10K strings of length 100
\ Checksum: sum of first char after reverse (which was last char before)

create buf 128 allot

: fill-buf ( n -- ) 100 0 do dup 26 mod 65 + buf i + c! loop drop ;
: reverse-buf 100 2 / 0 do buf i + c@ buf 99 i - + c@ buf i + c! buf 99 i - + c! loop ;
: bench-strrev ( -- sum )
  0
  10000 0 do
    i fill-buf
    reverse-buf
    buf c@ +
  loop ;

: main bench-strrev . cr ;
