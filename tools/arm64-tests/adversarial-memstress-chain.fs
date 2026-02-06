\ expect: 42
\ ADVERSARIAL: Chain multiple operations: allot, fill, move, verify
\ Tests complex memory operation sequence
: main
  here           \ save base
  8 allot        \ alloc first buffer
  here           \ save second base
  8 allot        \ alloc second buffer

  \ Stack: ( buf1 buf2 )
  over 8 42 fill           \ fill buf1 with 42
  over over 8 move         \ move buf1 to buf2
  nip c@                   \ read first byte of buf2 (should be 42)
;
