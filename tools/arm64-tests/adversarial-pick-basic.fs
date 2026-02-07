\ expect: 0
\ Adversarial: pick basic tests (0-4 pick)
\ 0 pick = dup, 1 pick = over, etc.
: main
  10 20 30 40 50
  \ Stack: 10 20 30 40 50
  0 pick 50 - abs       \ 0 pick = 50 (dup)
  swap drop             \ remove the picked value
  1 pick 40 - abs +     \ 1 pick = 40 (over)
  swap drop
  2 pick 30 - abs +     \ 2 pick = 30
  swap drop
  3 pick 20 - abs +     \ 3 pick = 20
  swap drop
  4 pick 10 - abs +     \ 4 pick = 10
  swap drop
  \ Clean up remaining stack
  drop drop drop drop drop
;
