\ expect: 88
\ Test fill: fill memory with a byte value
: main
  here 4 88 fill   \ fill 4 bytes with 'X' (88)
  here c@          \ read first byte (should be 88)
;
