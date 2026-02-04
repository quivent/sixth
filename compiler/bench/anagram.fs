\ expected: 50000
\ Anagram detection benchmark - check anagram pairs, run 50000 times

create word1 32 allot
create word2 32 allot
create counts1 26 allot
create counts2 26 allot

: init-words ( -- )
  s" listen" drop word1 6 cmove
  s" silent" drop word2 6 cmove ;

: clear-counts ( -- )
  counts1 26 0 fill
  counts2 26 0 fill ;

: count-char ( c counts -- )
  swap [char] a - + dup c@ 1+ swap c! ;

: count-word ( addr len counts -- )
  -rot 0 do
    over i + c@ over count-char
  loop 2drop ;

: compare-counts ( -- flag )
  -1
  26 0 do
    counts1 i + c@ counts2 i + c@ <> if drop 0 leave then
  loop ;

: anagram? ( -- flag )
  clear-counts
  word1 6 counts1 count-word
  word2 6 counts2 count-word
  compare-counts ;

: main ( -- )
  init-words
  0
  50000 0 do
    anagram? if 1+ then
  loop
  . cr ;
