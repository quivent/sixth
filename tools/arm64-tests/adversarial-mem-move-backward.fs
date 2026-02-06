\ expect: ABCDE
\ ADVERSARIAL: Move with dst < src (forward copy is safe)
\ When dst < src, forward copy works correctly
\ Test non-overlapping copy to verify basic move functionality

: main
  here 16 allot           \ allocate buffer
  here 16 -               \ base address
  s" ABCDE" drop over 5 move  \ copy "ABCDE" to here
  5 type                  \ print result
  0                       \ exit 0
;
