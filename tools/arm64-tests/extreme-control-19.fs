\ expect: 42
\ Test: IF inside IF inside IF with ELSE at each level selecting different paths
\ Each path must correctly jump to its matching THEN

: triple-branch ( a b c -- result )
  rot          \ ( b c a )
  0> if        \ a > 0 ?
    swap       \ ( c b )
    0> if      \ b > 0 ?
      0> if    \ c > 0 ?
        111    \ a>0, b>0, c>0
      else
        110    \ a>0, b>0, c<=0
      then
    else
      drop
      100      \ a>0, b<=0
    then
  else
    2drop
    0          \ a<=0
  then
;

: main
  \ Test path: a=1 (>0), b=1 (>0), c=-1 (<=0) -> should get 110
  1 1 -1 triple-branch
  \ 110 - 68 = 42
  68 -
;
