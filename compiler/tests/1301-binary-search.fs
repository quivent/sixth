\ expect: 2
create arr 40 allot
: arr@ ( i -- val ) 8 * arr + @ ;
variable lo  variable hi  variable target  variable result
: bsearch ( val n -- idx )
  1- hi !  0 lo !  target !  -1 result !
  begin lo @ hi @ <= while
    lo @ hi @ + 2 /
    dup arr@ target @ = if
      result !  hi @ 1+ lo !
    else
      dup arr@ target @ < if
        1+ lo !
      else
        1- hi !
      then
    then
  repeat
  result @ ;
: main
  10 0 8 * arr + !
  20 1 8 * arr + !
  25 2 8 * arr + !
  30 3 8 * arr + !
  40 4 8 * arr + !
  25 5 bsearch . cr ;
