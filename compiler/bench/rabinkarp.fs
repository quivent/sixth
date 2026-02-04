\ expected: 0
\ Rabin-Karp search, 100K searches
\ Checksum: sum of found positions

create text 64 allot
create pattern 8 allot
variable pat-len
101 constant PRIME
256 constant BASE

: fill-text ( n -- ) 32 0 do i over + 26 mod 97 + text i + c! loop drop ;
: fill-pat ( n -- ) 3 0 do over i + 26 mod 97 + pattern i + c! loop drop 3 pat-len ! ;

: pat-hash ( -- h )
  0 pat-len @ 0 do BASE * pattern i + c@ + PRIME mod loop ;

: text-hash ( pos -- h )
  0 pat-len @ 0 do BASE * text 2 pick i + + c@ + PRIME mod loop swap drop ;

: match-at ( pos -- 0|1 )
  1 pat-len @ 0 do text over i + + c@ pattern i + c@ <> if drop 0 leave then loop swap drop ;

: rk-h ( -- h ) 1 pat-len @ 1- 0 do BASE * PRIME mod loop ;

: rk-search ( -- pos )
  pat-hash
  0 text-hash
  over over = if 0 match-at if 2drop 0 exit then then
  rk-h
  32 pat-len @ - 0 do
    text j + c@ over * PRIME mod -
    PRIME + PRIME mod
    BASE * text j pat-len @ + + c@ + PRIME mod
    2 pick over = if
      j 1+ match-at if
        2drop drop j 1+ unloop exit
      then
    then
  loop
  2drop drop -1 ;

: bench-rk ( -- sum )
  0
  100000 0 do
    i fill-text
    i fill-pat
    rk-search
    dup 0< if drop else + then
  loop ;

: main bench-rk . cr ;
