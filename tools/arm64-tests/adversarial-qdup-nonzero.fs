\ Adversarial: ?dup with non-zero must duplicate
\ Tests that CBZ does NOT branch when TOS is non-zero
\ If non-zero, ?dup should leave x x, then = gives -1 (true)
\ expect: 247
: test1 ( -- n )
  1 ?dup        \ should give 1 1
  = if 41 else 0 then ;  \ 1=1 is true, so 41

: test2 ( -- n )
  -1 ?dup       \ should give -1 -1
  = if 41 else 0 then ;  \ -1=-1 is true, so 41

: test3 ( -- n )
  100 ?dup      \ should give 100 100
  = if 41 else 0 then ;  \ 100=100 is true, so 41

: test4 ( -- n )
  -9223372036854775808 ?dup  \ MIN_INT64, should duplicate
  = if 41 else 0 then ;

: test5 ( -- n )
  9223372036854775807 ?dup   \ MAX_INT64, should duplicate
  = if 41 else 0 then ;

: test6 ( -- n )
  42 ?dup       \ 42 42
  drop          \ 42
;

: main
  test1 test2 + test3 + test4 + test5 + test6 +
  \ 41 + 41 + 41 + 41 + 41 + 42 = 247
;
