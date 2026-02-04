\ expected: 20
\ Narcissistic numbers benchmark - count narcissistic 1-1000000, run 5 times

: count-digits ( n -- count )
  0 swap
  begin dup while 10 / swap 1+ swap repeat drop ;

: ipow ( base exp -- result )
  dup 0= if 2drop 1 exit then
  1 swap 0 do over * loop nip ;

: narcissistic? ( n -- flag )
  dup count-digits
  0 rot
  begin dup while
    10 /mod swap
    3 pick ipow
    rot + swap
  repeat drop
  swap drop = ;

: count-narcissistic ( limit -- count )
  0 swap 1+ 1 do
    i narcissistic? if 1+ then
  loop ;

: main ( -- )
  0
  5 0 do
    drop 1000000 count-narcissistic
  loop
  . cr ;
