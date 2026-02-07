\ expect: 0
\ adversarial-2over-nested.fs - Test 2over inside control structures
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )

: in-if ( -- n )
  1 2 3 4
  1 if
    2over        \ stack: 1 2 3 4 1 2
    + + + + +    \ sum all 6: 1+2+3+4+1+2=13
  else 999 then
;

: in-begin ( -- n )
  1 2 3 4
  3
  begin dup 0> while
    >r 2over 2drop r> 1-
  repeat
  drop + + +     \ sum: 1+2+3+4=10
;

: deeply-nested ( -- n )
  1 2 3 4
  1 if 1 if 1 if
    2over + + + + +  \ 13
  else 0 then else 0 then else 0 then
;

: main
  in-if in-begin + deeply-nested +
  36 - abs
;
