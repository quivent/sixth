\ expect: 1
\ Signed MOD with negative dividend
\ ARM64 uses truncated (symmetric) division, not floored
\ -7 / 3 = -2 (truncated), remainder = -7 - (-2*3) = -1
: main -7 3 mod 0< if 1 else 0 then ;
