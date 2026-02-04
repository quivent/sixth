\ expected: 1000000
\ Compare 1M string pairs
\ Checksum: count of equal comparisons

create buf1 32 allot
create buf2 32 allot

: fill-str ( buf n -- ) swap 8 0 do 2dup i + c! loop 2drop ;
: cmp-str ( -- 0|1 ) 1 8 0 do buf1 i + c@ buf2 i + c@ <> if drop 0 leave then loop ;

: bench-strcmp ( -- count )
  0
  1000000 0 do
    buf1 i 1000 / fill-str
    buf2 i 1000 / fill-str
    cmp-str +
  loop ;

: main bench-strcmp . cr ;
