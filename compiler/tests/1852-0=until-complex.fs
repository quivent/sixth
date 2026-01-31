\ expect: 6 3 1
\ 0=until with halving: count down by dividing by 2 until zero
: main 6 begin dup . 2/ dup 0=until drop cr ;
