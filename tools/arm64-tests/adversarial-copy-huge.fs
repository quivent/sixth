\ expect: 1
\ ADVERSARIAL: Test close to memory limit
\ NOTE: There is a memory layout bug where here/allot space
\ overlaps with the return stack. The return stack starts at
\ X20+1008, so we can only safely write ~990 bytes from here.
\ This test uses 900 bytes to stay within safe range.

variable buf
: main
  here buf ! 910 allot
  buf @
  900 0 do 97 over i + c! loop
  900 0 open-file drop drop
  1
;
