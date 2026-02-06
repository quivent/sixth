\ expect: 0
\ Test: String literals mixed with control flow and arithmetic
\ Strings inside conditionals and loops - register pressure nightmare

variable total

: countbyte ( addr u -- sum )
  0 -rot
  over + swap do
    i c@ +
  loop ;

: process ( -- )
  0 total !
  s" ABC" countbyte total +!
  total @ 100 > if
    s" XYZ" countbyte total +!
  else
    s" 123" countbyte total +!
  then
  5 0 do
    i 2 mod 0= if
      s" ab" countbyte total +!
    then
  loop ;

: main
  process
  \ ABC = 65+66+67 = 198
  \ 198 > 100, so XYZ = 88+89+90 = 267
  \ 198 + 267 = 465
  \ Loop: i=0,2,4 add "ab" = 97+98 = 195 each = 585
  \ Total: 465 + 585 = 1050
  total @ 1050 = if 0 else 1 then ;
