\ expected: 25500000
\ Length of 1M strings
\ Checksum: sum of all lengths

create buf 64 allot

: fill-buf ( n -- ) dup 50 mod 1+ 0 do 65 buf i + c! loop 50 mod 1+ buf + 0 swap c! ;
: my-strlen ( -- len ) 0 begin buf over + c@ while 1+ repeat ;

: bench-strlen ( -- sum )
  0
  1000000 0 do
    i fill-buf
    my-strlen +
  loop ;

: main bench-strlen . cr ;
