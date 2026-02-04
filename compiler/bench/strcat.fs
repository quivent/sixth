\ expected: 77499560
\ Concatenate 100K times
\ Checksum: sum of final string first char value * length

create buf 256 allot
variable len

: reset-buf 0 len ! ;
: append-char ( c -- ) len @ buf + c! 1 len +! ;
: append-str ( n -- ) 65 n 26 mod + 10 0 do dup append-char loop drop ;

: bench-strcat ( -- sum )
  0
  100000 0 do
    reset-buf
    i append-str
    buf c@ len @ * +
  loop ;

: main bench-strcat . cr ;
