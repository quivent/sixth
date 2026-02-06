\ expect: 66
\ ADVERSARIAL: Test c@ and c! at odd (unaligned) addresses
\ Ensures byte operations work at any address alignment
\ Stores bytes at here+1, here+3, here+5 (odd offsets)

: main
  here 6 allot            \ allocate 6 bytes
  here 1+ 65 swap c!      \ store 'A' at here+1 (odd addr)
  here 3 + 66 swap c!     \ store 'B' at here+3 (odd addr)
  here 5 + 67 swap c!     \ store 'C' at here+5 (odd addr)
  here 3 + c@             \ fetch byte at here+3 (should be 66)
;
