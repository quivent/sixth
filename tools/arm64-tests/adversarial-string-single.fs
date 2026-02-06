\ expect: 0
\ /string on single char, skip it entirely
: main
  s" X"        \ ( addr 1 )
  1 /string    \ ( addr+1 0 )
  nip          \ ( 0 )
;
