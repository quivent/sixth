\ expected: 3734
\ Caesar cipher benchmark - encode string, sum ASCII values, 50000 times

create teststr 64 allot
create outstr 64 allot

: init-str ( -- )
  s" The quick brown fox jumps over lazy dog" drop teststr 39 cmove ;

: caesar-char ( c shift -- c' )
  over [char] A [char] Z 1+ within if
    swap [char] A - + 26 mod [char] A + exit
  then
  over [char] a [char] z 1+ within if
    swap [char] a - + 26 mod [char] a + exit
  then
  drop ;

: caesar ( addr len shift -- )
  -rot 0 do
    2dup i + c@ swap caesar-char
    outstr i + c!
  loop 2drop ;

: sum-out ( len -- sum )
  0 swap 0 do outstr i + c@ + loop ;

: main ( -- )
  init-str
  0
  50000 0 do
    drop
    teststr 39 5 caesar
    39 sum-out
  loop
  . cr ;
