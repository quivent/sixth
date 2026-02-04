\ expected: 5
\ Text wrap benchmark - wrap text at width, count lines, 50000 times

create text 128 allot

: init-text ( -- )
  s" The quick brown fox jumps over the lazy dog near the river bank" drop text 64 cmove ;

: wrap-count ( addr len width -- lines )
  1 >r
  0 0 do
    over i + c@ 32 = if
      drop i
    then
    dup 2 pick >= if
      r> 1+ >r
      drop 0
    else
      1+
    then
  loop
  2drop r> ;

: main ( -- )
  init-text
  0
  50000 0 do
    drop
    text 64 12 wrap-count
  loop
  . cr ;
