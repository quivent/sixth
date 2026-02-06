\ expect: 0
\ Test: ROT and -ROT are inverses
\ (a b c rot -rot) must equal (a b c)

: main
  111 222 333
  rot -rot          ( Should restore original order )
  333 - swap 222 - or swap 111 - or
  ( all diffs should be 0 )
;
