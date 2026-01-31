\ expect: 5 4 3 2 1
\ 1-nzloop: decrements TOS and loops while nonzero
\ Print TOS before each decrement
: main 5 begin dup . 1-nzloop drop cr ;
