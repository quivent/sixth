\ expect: 0 3 6 9 12
\ i from 0 to 4, print i*3 only if < 15 (all qualify)
: main
  5 0 do
    i 3 * dup 15 < if . else drop then
  loop cr ;
