\ expect: XYZ
\ count: verify full content access
: main
  here            \ destination for counted string
  3 over c!       \ store length=3
  1+ 88 over c!   \ 'X' at here+1
  1+ 89 over c!   \ 'Y' at here+2
  1+ 90 swap c!   \ 'Z' at here+3
  here count      \ ( addr len )
  type            \ should print "XYZ"
  0               \ exit 0
;
