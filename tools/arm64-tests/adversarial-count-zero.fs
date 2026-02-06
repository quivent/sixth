\ expect: 0
\ count with zero-length counted string
: main
  here         \ destination for counted string
  0 over c!    \ store length=0 at here
  here count   \ ( addr len )
  nip          \ ( len ) - should be 0
;
