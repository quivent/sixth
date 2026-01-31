\ expect: 10 50
create arr 40 allot
variable lo
variable hi
: main
  30 arr ! 10 arr 1 cells + ! 50 arr 2 cells + ! 20 arr 3 cells + ! 40 arr 4 cells + !
  arr @ dup lo ! hi !
  5 1 do
    arr i cells + @
    dup lo @ min lo !
    dup hi @ max hi !
    drop
  loop
  lo @ . hi @ . cr ;
