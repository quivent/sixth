\ expect: 5
\ /string with n=0: no adjustment
: main
  s" HELLO"    \ ( addr 5 )
  0 /string    \ ( addr 5 ) - unchanged
  nip          \ ( 5 )
;
