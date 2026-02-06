\ Adversarial Stack Test 09: Interleaved stack ops with arithmetic
\ expect: 0
\ Test stack operations mixed with arithmetic

: t-add-swap ( -- flag )
  1 2 3 +
  \ 1 5
  swap
  \ 5 1
  1 = swap 5 = and
  if 0 else 1 then ;

: t-mul-rot ( -- flag )
  2 3 4 *
  \ 2 12
  5 rot
  \ 12 5 2
  2 = swap 5 = and swap 12 = and
  if 0 else 1 then ;

: t-arith-chn ( -- flag )
  10 20 + 5 * 3 /
  \ (10+20)*5/3 = 150/3 = 50
  50 = if 0 else 1 then ;

: t-dup-arith ( -- flag )
  7 dup *
  \ 7 7 -> 49
  49 = if 0 else 1 then ;

: t-over-arith ( -- flag )
  3 4 over *
  \ 3 4 3 -> 3 12
  12 = swap 3 = and
  if 0 else 1 then ;

: t-complex ( -- flag )
  1 2 3 4 5
  + swap       \ 1 2 3 9 -> 1 2 9 3
  * rot        \ 1 2 27 -> 2 27 1
  - nip        \ 2 26 -> 26
  26 = if 0 else 1 then ;

: main
  t-add-swap
  t-mul-rot +
  t-arith-chn +
  t-dup-arith +
  t-over-arith +
  t-complex + ;
