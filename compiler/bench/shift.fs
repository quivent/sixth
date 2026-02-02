\ expected: 1
\ Shift operations stress

: main
  1 1000000000 0 do dup 1 lshift 1 rshift drop loop . cr ;
