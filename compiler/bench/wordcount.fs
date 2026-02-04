\ expected: 12
\ Word count benchmark - count words in text, 100000 times

create text 128 allot

: init-text ( -- )
  s" The quick brown fox jumps over the lazy dog near the river" drop text 60 cmove ;

: is-space? ( c -- flag )
  dup 32 = swap 9 = or ;

: count-words ( addr len -- count )
  0 -rot 0
  0 do
    over i + c@ is-space?
    if
      dup if drop 0 rot 1+ -rot then
    else
      drop -1
    then
  loop
  if 1+ then nip ;

: main ( -- )
  init-text
  0
  100000 0 do
    drop
    text 60 count-words
  loop
  . cr ;
