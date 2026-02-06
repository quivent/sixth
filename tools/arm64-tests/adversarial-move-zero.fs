\ expect: 255
\ ADVERSARIAL: Zero-length move (u=0)
\ Tests the CBZ X12, done branch for count=0
\ No bytes should be copied, destination unchanged
: main
  here 255 over c!    \ store 255 at here
  s" XYZ" drop        \ src = "XYZ"
  here                \ dst = here (contains 255)
  0 move              \ move 0 bytes
  here c@             \ should still be 255
;
