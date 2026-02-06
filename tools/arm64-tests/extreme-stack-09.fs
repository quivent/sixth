\ expect: 21
\ Test: over/swap interleaved stress
: main
  1 2 3 4 5 6
  over over over over over over
  swap swap swap swap swap swap
  over swap over swap over swap
  drop drop drop drop drop drop
  drop drop drop drop drop drop
  + + + + +
;
