\ expect: CDE
\ /string partial adjustment, verify content
: main
  s" ABCDE"    \ ( addr 5 )
  2 /string    \ ( addr+2 3 )
  type         \ should print "CDE"
  0            \ exit 0
;
