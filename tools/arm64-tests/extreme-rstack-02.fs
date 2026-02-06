\ expect: 5
\ Test: R@ reads TOS repeatedly without modifying rstack
: main
  5 >r
  r@ r@ r@ r@ r@ r@ r@ r@ r@ r@
  drop drop drop drop drop
  drop drop drop drop
  r>
;
