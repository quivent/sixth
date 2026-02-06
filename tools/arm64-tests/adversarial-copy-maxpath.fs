\ expect: 1
\ ADVERSARIAL: Test near-max path
\ NOTE: Memory layout limits us to ~990 bytes from here.
\ Using 500 bytes to test long paths within safe range.

variable buf
: main
  here buf ! 510 allot
  buf @
  47 over c!  116 over 1+ c!  109 over 2 + c!  112 over 3 + c!  47 over 4 + c!
  495 0 do 97 over 5 i + + c! loop
  500 0 open-file drop drop
  1
;
