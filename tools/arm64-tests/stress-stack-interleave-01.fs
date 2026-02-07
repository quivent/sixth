\ stress-stack-interleave-01.fs - Interleaved data/return stack operations
\ Tests: Alternating pushes/pops between data stack and return stack
\ Edge case: Register allocation conflicts, stack pointer confusion
\ expect: 55

\ This tests complex interleaving patterns that could confuse
\ register allocation or stack pointer management

: zigzag ( n -- result )
  \ Alternating data/return stack ops
  dup >r         \ data: n, return: n
  1+ dup >r      \ data: n+1, return: n n+1
  1+ dup >r      \ data: n+2, return: n n+1 n+2
  drop           \ data: empty
  r>             \ data: n+2
  r> +           \ data: n+2 + n+1 = 2n+3
  r> +           \ data: 2n+3 + n = 3n+3
;

: mix-ops ( a b c -- result )
  \ Complex mixing of stack operations
  rot            \ b c a
  >r             \ b c, R: a
  swap           \ c b
  over           \ c b c
  >r             \ c b, R: a c
  +              \ c+b
  r>             \ c+b c, R: a
  r>             \ c+b c a
  + +            \ c+b+c+a = a+2c+b
;

: verify-order ( -- flag )
  \ Push 1 2 3 to data, 4 5 6 to return, verify retrieval order
  1 2 3          \ data: 1 2 3
  4 >r 5 >r 6 >r \ return: 4 5 6 (6 on top)
  \ Now pop return stack
  r>             \ data: 1 2 3 6
  6 = if
    r>           \ data: 1 2 3 5
    5 = if
      r>         \ data: 1 2 3 4
      4 = if
        \ Check data stack: should be 1 2 3
        3 = if
          2 = if
            1 = if 1 else 0 then
          else 0 then
        else 0 then
      else 0 then
    else 0 then
  else 0 then
;

: save-across ( a b -- a+b )
  \ Save one value, operate on other, restore
  >r             \ R: b, Stack: a
  dup +          \ Stack: a*2
  r>             \ Stack: a*2 b
  +              \ Stack: a*2+b
;

: multi-save ( -- result )
  \ Multiple saves and restores in sequence
  10 >r 20 >r 30 >r   \ R: 10 20 30
  r> r> r>            \ Stack: 30 20 10
  + +                 \ 30+20+10 = 60
;

: main
  \ Test zigzag: zigzag(10) should be 3*10+3 = 33
  10 zigzag
  33 = if
    \ Test verify-order
    verify-order
    1 = if
      \ Test save-across: save-across(5, 7) = 5*2+7 = 17
      5 7 save-across
      17 = if
        \ Test multi-save: should be 60
        multi-save
        60 = if
          55   \ Success!
        else 4 then
      else 3 then
    else 2 then
  else 1 then
;
