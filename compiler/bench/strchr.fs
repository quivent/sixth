\ expected: 4615411
\ Find char 1M times
\ Checksum: sum of found positions (0-indexed, -1 if not found)

create buf 32 allot

: fill-buf ( n -- ) 16 0 do i 65 + buf i + c! loop drop ;
: find-char ( c -- pos ) -1 16 0 do buf i + c@ over = if drop i leave then loop swap drop ;

: bench-strchr ( -- sum )
  0
  1000000 0 do
    i fill-buf
    i 26 mod 65 + find-char
    dup 0< if drop else + then
  loop ;

: main bench-strchr . cr ;
