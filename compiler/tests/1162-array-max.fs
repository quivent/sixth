\ expect: 99
create arr 40 allot
: main
  3 arr ! 99 arr cell+ ! 7 arr cell+ cell+ !
  arr @ arr cell+ @ max arr cell+ cell+ @ max . cr ;
