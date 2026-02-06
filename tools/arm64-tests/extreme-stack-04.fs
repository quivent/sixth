\ expect: 42
\ Test: 2dup/2drop chains - tests pair operations
: main
  1 2
  2dup 2dup 2dup 2dup 2dup
  2dup 2dup 2dup 2dup 2dup
  2dup 2dup 2dup 2dup 2dup
  2drop 2drop 2drop 2drop 2drop
  2drop 2drop 2drop 2drop 2drop
  2drop 2drop 2drop 2drop 2drop
  2drop
  42
;
