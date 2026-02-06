\ expect: 3
\ Test /string: adjust string pointer and length
: main
  s" ABCDE"    \ ( addr 5 )
  2 /string    \ ( addr+2 3 )
  nip          \ ( 3 ) - just keep the length
;
