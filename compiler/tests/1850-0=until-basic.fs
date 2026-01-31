\ expect: 3 2 1
\ 0=until: loops while TOS != 0, consumes TOS each iteration
\ Need to produce a new value each iteration
: main 3 begin dup . 1- dup 0=until drop cr ;
