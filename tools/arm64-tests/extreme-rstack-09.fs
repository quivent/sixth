\ expect: 110
\ Test: R@ used in arithmetic expressions
: main
  10 >r
  r@ r@ *
  r@ +
  r> drop
;
