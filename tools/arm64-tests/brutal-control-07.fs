\ expect: 12
\ Test: BEGIN/WHILE/REPEAT nested 3 deep with complex conditions

variable acc

: triple-nested ( a b c -- )
  \ Count iterations: a*b*c
  0 acc !
  begin
    over 0 >                \ while b > 0
  while
    begin
      dup 0 >               \ while c > 0
    while
      >r >r                 \ save b and c on return stack
      begin
        dup 0 >             \ while a > 0
      while
        acc @ 1+ acc !      \ increment accumulator
        1-                  \ a--
      repeat
      drop 3                \ reset a to 3
      r> r>                 \ restore b and c
      1-                    \ c--
    repeat
    drop 2                  \ reset c to 2
    swap 1- swap            \ b--
  repeat
  2drop drop                \ clean up a, b, c
;

: main
  3 2 2 triple-nested
  acc @
;
