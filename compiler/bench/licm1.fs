\ expected: 4200000000
\ Loop invariant code motion - hoist constant from loop

: main
  0 100000000 0 do
    42 +
  loop . cr ;
