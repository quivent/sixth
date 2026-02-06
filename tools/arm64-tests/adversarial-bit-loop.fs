\ Adversarial test: Bit manipulation in loop
\ Simple count using DO-LOOP with bitwise AND
\ Loop 8 times, each time check if bit is set and accumulate
\ expect: 8
variable cnt

: main
  0 cnt !
  8 0 do
    255 i rshift 1 and   \ check bit i of 255
    cnt @ + cnt !
  loop
  cnt @
;
