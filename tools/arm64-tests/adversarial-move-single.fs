\ expect: 88
\ ADVERSARIAL: Single byte move
\ Tests loop executes exactly once and terminates correctly
\ This catches off-by-one errors in the loop condition
: main
  s" X" drop          \ src = "X" (88 = 'X')
  here                \ dst
  1 move              \ copy exactly 1 byte
  here c@             \ should be 88 ('X')
;
