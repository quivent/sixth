\ expected: 142
\ Happy numbers benchmark - count happy numbers up to 1000, run 500 times

: sum-digit-squares ( n -- sum )
  0 swap
  begin dup while
    10 /mod swap dup * rot + swap
  repeat drop ;

: happy? ( n -- flag )
  50 0 do
    sum-digit-squares
    dup 1 = if drop -1 unloop exit then
  loop drop 0 ;

: count-happy ( limit -- count )
  0 swap 1 do
    i happy? if 1+ then
  loop ;

: main ( -- )
  0
  500 0 do
    drop 1000 count-happy
  loop
  . cr ;
