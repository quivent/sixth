\ expect: 0
\ Adversarial: MIN_INT vs MAX_INT - the ultimate signed test
\ If using unsigned, MIN_INT (0x8000...) > MAX_INT (0x7FFF...) which is WRONG
\ This is THE test that catches unsigned comparison bugs
\ Returns 0 if signed comparison is correct
: main
  1 63 lshift            \ MIN_INT
  1 63 lshift 1-         \ MAX_INT
  \ Stack: MIN_INT MAX_INT
  2dup min               \ min(MIN_INT, MAX_INT) should = MIN_INT
  1 63 lshift - abs      \ compare with MIN_INT
  -rot                   \ ( sum MIN_INT MAX_INT )
  max                    \ max(MIN_INT, MAX_INT) should = MAX_INT
  1 63 lshift 1- - abs + \ compare with MAX_INT
;
