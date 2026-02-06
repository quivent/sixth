\ expect: 200
\ ADVERSARIAL: Large buffer allocation and access
\ Tests that allot works for large buffers and access at far offsets
\ Allocates 1024 bytes, writes to last byte, reads it back

: main
  here                    \ save start
  1024 allot              \ allocate 1024 bytes
  here 1-                 \ get last byte address
  200 over c!             \ store 200 at last byte
  c@                      \ fetch last byte (should be 200)
;
