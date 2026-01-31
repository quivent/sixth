\ expect: 0 1 1 2 3 5 8 13 21 34
: main
  0 1
  10 0 do
    over .
    over +  swap
  loop
  2drop cr ;
