\ expected: 99999
\ Union-find 100K operations, count final components

100000 constant N
create parent N cells allot
create rank N cells allot

: find ( x -- root )
  begin
    dup cells parent + @
    2dup <> while
    nip
  repeat
  drop ;

: union ( x y -- )
  find swap find
  2dup = if 2drop exit then
  over cells rank + @
  over cells rank + @
  2dup < if
    2drop swap cells parent + !
  else
    2dup > if
      2drop cells parent + !
    else
      drop cells parent + !
      dup cells rank + @ 1+ swap cells rank + !
    then
  then ;

: init-uf ( -- )
  N 0 do
    i i cells parent + !
    0 i cells rank + !
  loop ;

: lcg ( n -- n' ) 1103515245 * 12345 + 2147483647 and ;

: do-unions ( -- )
  12345
  N 0 do
    lcg dup N mod
    over lcg N mod union
    lcg nip
  loop
  drop ;

: count-roots ( -- n )
  0 N 0 do
    i cells parent + @ i = if 1+ then
  loop ;

: main init-uf do-unions count-roots . cr ;
