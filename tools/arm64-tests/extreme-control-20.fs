\ expect: 99
\ Test: AGAIN with multiple IF-THEN inside, each with its own EXIT
\ Tests emit-exit from multiple locations within same infinite loop

: find-special ( start -- result )
  begin
    dup 3 mod 0= if
      dup 5 mod 0= if
        \ Divisible by both 3 and 5 (i.e., 15)
        exit
      then
    then
    dup 99 = if
      exit   \ Safety exit at 99
    then
    1+
  again
;

: main
  1 find-special   \ starts at 1, first number divisible by 15 is 15
  \ But wait - 15 mod 3 = 0 AND 15 mod 5 = 0, so should exit at 15
  84 +             \ 15 + 84 = 99
;
