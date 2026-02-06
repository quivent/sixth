\ expect: 100
\ Test: Data stack values pushed to rstack, popped back
: main
  10 20 30 40
  >r >r >r >r
  r> r> r> r>
  + + +
;
