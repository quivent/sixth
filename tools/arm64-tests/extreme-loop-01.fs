\ expect: 42
\ Zero-iteration loop where start > limit (backward direction)
\ Standard Forth: start >= limit means zero iterations for LOOP
: main
  42
  5 10 do             \ limit=5, start=10, backwards - should skip
    drop 99           \ should NEVER execute
  loop
;
