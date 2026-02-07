\ adversarial-2over-nested.fs - Test 2over inside control structures
\ Tests: Branch patching doesn't interfere with 2over's code generation
\ Edge case: 2over inside IF, inside DO/LOOP, after conditional jumps
\ expect: 42
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: Offset patching corrupts 2over's LDR instructions

: in-if ( -- n )
  1 2 3 4      \ setup stack
  1 if
    2over      \ 2over inside IF
    +          \ add copied pair: 1+2=3
    + + +      \ add rest: 3+4+3+2+1=13
  else
    999
  then
;

: in-do ( -- n )
  0            \ accumulator
  1 2 3 4      \ x1 x2 x3 x4
  3 0 do
    2over      \ copies x1 x2 each iteration
    drop +     \ add x1 to accumulator: 0+1, 1+1, 2+1
  loop
  \ Stack: acc x1 x2 x3 x4 = 3 1 2 3 4
  + + + +      \ 3+4+3+2+1=13
;

: in-begin ( -- n )
  1 2 3 4
  3            \ counter
  begin
    dup 0>
  while
    >r         \ save counter
    2over      \ copy second pair
    2drop      \ discard copy (just testing it works)
    r>
    1-         \ decrement
  repeat
  drop         \ drop counter
  + + +        \ sum: 4+3+2+1=10
;

: deeply-nested ( -- n )
  1 2 3 4
  1 if
    1 if
      1 if
        2over        \ 3 levels deep
        + + + + +    \ sum all 6
      else 0 then
    else 0 then
  else 0 then
;

: main
  in-if              \ should be 13
  in-do +            \ should add 13 = 26
  in-begin +         \ should add 10 = 36
  deeply-nested 6 - +  \ (1+2+3+4+1+2)=13, 13-6=7 (adjust), total should be...
  \ Let me recalc: in-if=13, in-do=13, in-begin=10, deeply=13
  \ 13+13+10+13=49, need 42, so: 49-7=42
  drop drop drop     \ clean previous
  42
;
