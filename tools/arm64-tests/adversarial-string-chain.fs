\ expect: E
\ Chained /string calls
: main
  s" ABCDE"    \ ( addr 5 )
  1 /string    \ ( addr+1 4 ) = "BCDE"
  2 /string    \ ( addr+3 2 ) = "DE"
  1 /string    \ ( addr+4 1 ) = "E"
  type         \ should print "E"
  0            \ exit 0
;
