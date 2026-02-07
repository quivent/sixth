\ expect: 0
\ Adversarial: pick basic tests (0-4 pick)
\ 0 pick = dup, 1 pick = over, etc.
: main
  10 20 30 40 50
  \ Stack: 10 20 30 40 50
  0 pick 50 - abs       \ 0 pick should be 50 (dup)
  1 pick 40 - abs +     \ 1 pick should be 40 (over) - note: stack changed
  \ Now stack is: 10 20 30 40 50 <picked> <sum>
  \ Need fresh setup for each test
  drop drop drop drop drop drop drop

  \ Test each pick individually and accumulate errors
  10 20 30 40 50
  0 pick 50 - abs   \ err0
  >r drop drop drop drop drop r>

  10 20 30 40 50
  1 pick 40 - abs   \ err1
  >r drop drop drop drop drop r> +

  10 20 30 40 50
  2 pick 30 - abs   \ err2
  >r drop drop drop drop drop r> +

  10 20 30 40 50
  3 pick 20 - abs   \ err3
  >r drop drop drop drop drop r> +

  10 20 30 40 50
  4 pick 10 - abs   \ err4
  >r drop drop drop drop drop r> +
;
