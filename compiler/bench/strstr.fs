\ expected: 0
\ Find substring 100K times
\ Checksum: sum of found positions

create haystack 64 allot
create needle 8 allot

: fill-hay ( n -- ) 32 0 do i n + 26 mod 65 + haystack i + c! loop ;
: fill-needle ( n -- ) 4 0 do n 26 mod 65 + needle i + c! loop ;
: match-at ( pos -- 0|1 ) 1 4 0 do haystack over i + + c@ needle i + c@ <> if drop 0 leave then loop swap drop ;
: find-sub ( -- pos ) -1 29 0 do i match-at if drop i leave then loop ;

: bench-strstr ( -- sum )
  0
  100000 0 do
    i fill-hay
    i fill-needle
    find-sub
    dup 0< if drop else + then
  loop ;

: main bench-strstr . cr ;
