\ expected: 0
\ KMP search, 100K searches
\ Checksum: sum of found positions

create text 64 allot
create pattern 8 allot
create lps 8 cells allot
variable pat-len

: fill-text ( n -- ) 32 0 do i over + 26 mod 97 + text i + c! loop drop ;
: fill-pat ( n -- ) 3 0 do over i + 26 mod 97 + pattern i + c! loop drop 3 pat-len ! ;

: compute-lps ( -- )
  0 lps !
  0 1
  begin dup pat-len @ < while
    pattern over + c@ pattern 2 pick + c@ = if
      swap 1+ dup lps 3 pick cells + !
      swap 1+
    else
      over 0> if
        swap lps swap 1- cells + @ swap
      else
        0 lps over cells + !
        1+
      then
    then
  repeat 2drop ;

: kmp-search ( -- pos )
  compute-lps
  0 0
  begin over 32 < while
    text over + c@ pattern over + c@ = if
      1+ swap 1+ swap
      dup pat-len @ = if
        drop pat-len @ - exit
      then
    else
      dup 0> if
        lps over 1- cells + @
      else
        swap 1+ swap
      then
    then
  repeat 2drop -1 ;

: bench-kmp ( -- sum )
  0
  100000 0 do
    i fill-text
    i fill-pat
    kmp-search
    dup 0< if drop else + then
  loop ;

: main bench-kmp . cr ;
