\ expect: 150
\ Test: Push three, then R@ all three without popping (all return 30)
: main
  10 >r 20 >r 30 >r
  r@ r@ r@ r@ r@
  + + + +
  r> r> r>
  drop drop drop
;
