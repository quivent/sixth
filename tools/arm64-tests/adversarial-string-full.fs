\ expect: 0
\ /string skip entire string (n=u)
: main
  s" ABCDE"    \ ( addr 5 )
  5 /string    \ ( addr+5 0 )
  nip          \ ( 0 )
;
