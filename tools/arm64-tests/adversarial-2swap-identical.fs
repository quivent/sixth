\ expect: 0
\ Adversarial: 2swap with all identical values
\ Catches bugs that rely on unique values
: main
  42 42 42 42
  2swap
  \ All should still be 42
  42 - abs
  swap 42 - abs +
  swap 42 - abs +
  swap 42 - abs +
;
