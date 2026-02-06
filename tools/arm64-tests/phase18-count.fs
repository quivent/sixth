\ expect: 3
\ Test count: convert counted string to addr/len
\ A counted string has length byte first, then chars
: main
  here         \ destination for counted string
  3 over c!    \ store length=3 at here
  1+ 65 over c! 1+ 66 over c! 1+ 67 swap c!  \ store "ABC"
  here count   \ ( addr len )
  nip          \ ( len ) - should be 3
;
