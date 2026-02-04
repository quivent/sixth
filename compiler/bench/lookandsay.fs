\ expected: 507345
\ Look-and-say sequence - describe previous term (15 iterations)

50000 constant MAXLEN
create buf1 MAXLEN allot
create buf2 MAXLEN allot

variable src
variable dst
variable slen
variable dlen

: init-seq ( -- )
  buf1 src !  buf2 dst !
  1 buf1 c!  1 slen ! ;

: swap-bufs ( -- )
  src @ dst @ src ! dst !
  dlen @ slen ! 0 dlen ! ;

: next-seq ( -- )
  0 dlen !
  0                             \ i=0
  begin dup slen @ < while
    dup src @ + c@              \ i char
    1                           \ i char count
    begin
      2 pick 1+ slen @ < if
        2 pick 1+ src @ + c@ 2 pick = if
          1+ swap 1+ swap 1
        else 0 then
      else 0 then
    while repeat
    \ Store count and char
    over 48 + dst @ dlen @ + c!
    dlen @ 1+ dlen !
    48 + dst @ dlen @ + c!
    dlen @ 1+ dlen !
    nip
  repeat drop
  swap-bufs ;

: seq-sum ( -- n )
  0 slen @ 0 do src @ i + c@ + loop ;

: main
  init-seq
  15 0 do next-seq loop
  seq-sum . cr ;
