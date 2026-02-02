\ expected: 500000000
\ Pure control flow stress - minimal body

: main
  0 1000000000 0 do i 1 and if 1+ then loop . cr ;
