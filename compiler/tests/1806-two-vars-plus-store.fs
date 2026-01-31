\ expect: 3 7
create a 8 allot
create b 8 allot
: main
  0 a ! 0 b !
  1 a +! 2 a +! 3 b +! 4 b +!
  a @ . b @ . ;
