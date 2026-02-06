\ expect: 0
\ Test: INVERT and combined bitwise operations

: main
  \ INVERT tests - bitwise NOT
  0 invert -1 <> if 1 exit then             \ NOT 0 = -1 (all bits set)
  -1 invert 0 <> if 2 exit then             \ NOT -1 = 0

  \ INVERT with specific bit patterns
  1 invert -2 <> if 3 exit then             \ NOT 1 = -2
  255 invert -256 <> if 4 exit then         \ NOT 255 = -256

  \ Double INVERT = identity
  12345 invert invert 12345 <> if 5 exit then
  -9999 invert invert -9999 <> if 6 exit then

  \ De Morgan's laws: NOT (A AND B) = (NOT A) OR (NOT B)
  240 15 and invert                         \ NOT (240 AND 15) = NOT 0 = -1
  240 invert 15 invert or                   \ (NOT 240) OR (NOT 15)
  <> if 7 exit then

  \ NOT (A OR B) = (NOT A) AND (NOT B)
  240 15 or invert                          \ NOT (240 OR 15) = NOT 255
  240 invert 15 invert and                  \ (NOT 240) AND (NOT 15)
  <> if 8 exit then

  0
;
