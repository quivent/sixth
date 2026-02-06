\ Adversarial test: Control flow with stack manipulation
\ Tests that stack ops work correctly inside control structures
\ expect: 42

: main
  10 20 12    \ stack: 10 20 12
  dup if      \ 12 is true, stack: 10 20 12
    over if   \ 20 is true, stack: 10 20 12
      rot dup -rot  \ rotate to get 10, dup, rotate back: 10 20 12 10
      if      \ 10 is true, stack: 10 20 12
        + +   \ 10 + 20 + 12 = 42
      else 0 then
    else drop drop 0 then
  else drop drop 0 then
;
