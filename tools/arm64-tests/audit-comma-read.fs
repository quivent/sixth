\ expect: 21
\ Test storing and reading back a value, verify low bits
\ Store 123456789, read back, mod 256 should be 21 (0x75ACD15 & 0xFF = 0x15 = 21)
: main here 123456789 , @ 256 mod ;
