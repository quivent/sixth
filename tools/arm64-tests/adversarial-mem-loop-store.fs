\ expect: 99
\ ADVERSARIAL: Memory store operations in tight loop
\ Tests that repeated ! operations don't corrupt state
\ Stores values 0-99 to same location, final value should be 99

variable accum

: main
  100 0 do
    i accum !             \ store current index
  loop
  accum @                 \ should be 99 (last value stored)
;
