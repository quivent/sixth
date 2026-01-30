\ Test: abs inside until loop → 3 2 1
: main -3 begin dup abs . 1+ dup 0 > until drop cr ;
