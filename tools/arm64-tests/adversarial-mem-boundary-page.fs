\ expect: 42
\ ADVERSARIAL: Test memory access at various offsets
\ Tests accessing memory at different alignments within a buffer
\ Verifies no crashes or corruption at boundary conditions

: main
  here                    \ base address
  256 allot               \ allocate a decent buffer
  here 256 -              \ back to start
  dup 0 + 42 swap c!      \ write at offset 0
  dup 7 + 43 swap c!      \ write at offset 7 (unaligned before cell)
  dup 8 + 44 swap c!      \ write at offset 8 (aligned)
  dup 255 + 45 swap c!    \ write at last offset
  c@                      \ read offset 0 (should be 42)
;
