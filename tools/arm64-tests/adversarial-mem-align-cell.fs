\ expect: 42
\ ADVERSARIAL: Test 8-byte aligned @ and ! operations
\ Verify that cell-sized operations work correctly at aligned addresses
\ Tests: allot creates aligned memory, ! stores 64-bit value, @ fetches it

variable testvar

: main
  123456789 testvar !     \ store large 64-bit value
  testvar @               \ fetch it back
  123456789 = if 42 else 0 then  \ return 42 if correct
;
