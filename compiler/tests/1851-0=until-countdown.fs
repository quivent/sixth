\ expect: 10 8 6 4 2
\ 0=until: countdown by 2s from 10
: main 10 begin dup . 2- dup 0=until drop cr ;
