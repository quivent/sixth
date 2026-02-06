\ expect: 65
\ Test move: copy bytes from src to dst
: main
  s" ABC" drop   \ get address of "ABC"
  here          \ destination
  3 move        \ copy 3 bytes
  here c@       \ read first byte (should be 'A' = 65)
;
