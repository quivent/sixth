\ expect: 3 1 20
\ Pick after complex stack manipulation
: main
  \ Test 1: pick after swap/rot
  1 2 3 4 5
  swap        \ 1 2 3 5 4
  rot         \ 1 2 5 4 3
  0 pick . cr \ 3
  drop drop drop drop drop

  \ Test 2: verify 1 pick = over
  1 2
  1 pick      \ 1 2 1
  nip nip     \ 1
  . cr

  \ Test 3: pick then arithmetic
  5 10 15 20 25
  3 pick      \ 5 10 15 20 25 10
  2 *         \ 5 10 15 20 25 20
  nip nip nip nip nip
  . cr
;
