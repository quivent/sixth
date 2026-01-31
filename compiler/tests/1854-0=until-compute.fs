\ expect: 16 8 4 2 1
\ 0=until for halving down from power of 2
: main 16 begin dup . 2/ dup 0=until drop cr ;
