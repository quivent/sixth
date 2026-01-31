\ expect: 3
\ Binary search in sorted array using loop
create arr 56 allot
: a@ ( i -- val ) cells arr + @ ;
variable lo variable hi variable mid
: bsearch ( val -- idx|-1 )
  0 lo !  6 hi !
  begin lo @ hi @ <= while
    lo @ hi @ + 2/ mid !
    dup mid @ a@ = if drop mid @ exit then
    dup mid @ a@ < if
      mid @ 1- hi !
    else
      mid @ 1+ lo !
    then
  repeat
  drop -1 ;
: main
  10 0 cells arr + !
  20 1 cells arr + !
  30 2 cells arr + !
  40 3 cells arr + !
  50 4 cells arr + !
  60 5 cells arr + !
  70 6 cells arr + !
  40 bsearch . cr ;
