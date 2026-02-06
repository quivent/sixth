\ expect: 255
\ count with max length (255)
: main
  here         \ destination for counted string
  255 over c!  \ store length=255 at here
  here count   \ ( addr len )
  nip          \ ( len ) - should be 255
;
