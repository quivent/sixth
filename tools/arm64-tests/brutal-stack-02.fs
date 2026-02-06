\ expect: 0
\ Test: SWAP correctness with distinct values
\ Naive implementations might corrupt one value

: main
  12345678 87654321 swap
  ( now: 87654321 12345678 )
  12345678 - swap 87654321 - or
  ( both diffs should be 0, or = 0 )
;
