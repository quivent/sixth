\ expect: A
\ count single char string, verify content
: main
  here         \ destination for counted string
  1 over c!    \ store length=1
  1+ 65 swap c! \ store 'A' at here+1
  here count   \ ( addr 1 )
  type         \ should print "A"
  0            \ exit 0
;
