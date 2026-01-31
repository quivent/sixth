\ expect: 0 1 0 1 0 1 0 1
\ 170 = 10101010 in binary
\ Extract LSB first: bit0=0, bit1=1, bit2=0, bit3=1, bit4=0, bit5=1, bit6=0, bit7=1
: main
  170
  8 0 do
    dup 1 and .
    1 rshift
  loop drop cr ;
