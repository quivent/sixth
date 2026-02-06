\ expect: 255
\ ADVERSARIAL: Fill with 0xFF and verify all bytes
\ Tests fill with maximum byte value across multiple bytes
\ Verifies each byte independently to catch sign extension bugs

: main
  here                    \ save address
  4 255 fill              \ fill 4 bytes with 255
  here c@                 \ first byte (should be 255)
  here 1+ c@ = if         \ second byte also 255?
    here 2 + c@ 255 = if  \ third byte also 255?
      here 3 + c@ 255 = if \ fourth byte also 255?
        255               \ all correct
      else 0 then
    else 0 then
  else 0 then
;
