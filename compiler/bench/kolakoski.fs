\ expected: 50008
\ Kolakoski sequence - self-describing sequence of 1s and 2s

100000 constant MAXLEN
create seq MAXLEN allot

: gen-kolakoski ( n -- )
  seq 1 over c!
  seq 1+ 2 over c!
  seq 2 + 2 swap c!     \ seq = [1, 2, 2, ...]
  3                     \ len=3
  1                     \ i=1 (index for run lengths)
  2                     \ curval=2 (current value to write)
  begin 2 pick MAXLEN < while
    \ Get run length from seq[i]
    2 pick seq + c@     \ len i curval runlen
    0 do
      3 pick MAXLEN < if
        3 pick seq + 2 pick swap c!
        rot 1+ rot rot
      then
    loop
    swap 1+ swap        \ i++
    dup 1 = if drop 2 else drop 1 then  \ toggle curval
  repeat
  drop drop drop ;

: count-ones ( n -- count )
  0 swap 0 do
    seq i + c@ 1 = if swap 1+ swap then
  loop drop ;

: main
  MAXLEN gen-kolakoski
  MAXLEN count-ones . cr ;
