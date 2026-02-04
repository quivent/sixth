\ expected: 99999
\ BST find 100K times, sum of depths searched

100000 constant N
create left N cells allot
create right N cells allot
create key N cells allot
variable root
variable ncount

: insert ( val -- )
  ncount @ 0= if
    dup key 0 cells + !
    -1 left 0 cells + !
    -1 right 0 cells + !
    1 ncount !
    drop exit
  then
  0
  begin
    over key over cells + @
    2 pick < if
      dup cells right + @
      dup 0< if
        drop
        ncount @ over cells right + !
        2 pick ncount @ cells key + !
        -1 ncount @ cells left + !
        -1 ncount @ cells right + !
        1 ncount +!
        2drop exit
      then
      nip
    else
      dup cells left + @
      dup 0< if
        drop
        ncount @ over cells left + !
        2 pick ncount @ cells key + !
        -1 ncount @ cells left + !
        -1 ncount @ cells right + !
        1 ncount +!
        2drop exit
      then
      nip
    then
  again ;

: find-depth ( val -- depth )
  0 0  \ depth node
  begin
    over key over cells + @
    3 pick = if 2drop nip exit then
    over key over cells + @
    3 pick < if
      dup cells right + @
      dup 0< if 2drop nip exit then
      nip swap 1+ swap
    else
      dup cells left + @
      dup 0< if 2drop nip exit then
      nip swap 1+ swap
    then
  again ;

: lcg ( n -- n' ) 1103515245 * 12345 + 2147483647 and ;

: build-tree ( -- )
  0 ncount !
  50000 insert
  12345
  N 1 do
    lcg dup N mod insert
  loop
  drop ;

: search-all ( -- sum )
  0
  12345
  N 1 do
    lcg dup N mod find-depth rot + swap
  loop
  drop ;

: main build-tree search-all . cr ;
