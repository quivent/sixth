\ expect: 89
\ Test: Mixed everything - the gauntlet
: main
  1 2 3 4 5 6 7 8 9 10
  2dup 2drop
  rot -rot swap
  dup drop
  over swap drop
  10 0 do
    dup 1+
  loop
  drop drop drop drop drop
  drop drop drop drop drop
  drop drop drop drop
  + + + + + + + +
  45 - 89 +
;
