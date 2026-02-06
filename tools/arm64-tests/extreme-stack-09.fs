\ expect: 21
\ Test: over/swap interleaved stress
\ Stack: 6 + 6(over) + 3(over swap pairs) = 15, need 6 for 5 adds
: main
  1 2 3 4 5 6
  over over over over over over
  swap swap swap swap swap swap
  over swap over swap over swap
  drop drop drop drop drop drop
  drop drop drop
  + + + + +
;
