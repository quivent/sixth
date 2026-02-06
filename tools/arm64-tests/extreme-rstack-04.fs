\ expect: 55
\ Test: >R R> inside DO-LOOP - must not corrupt loop indices
: main
  0
  11 1 do
    i >r
    r@ +
    r> drop
  loop
;
