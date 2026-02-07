\ stress-stack-rstack-01.fs - Return stack stress across word boundaries
\ Tests: >r/r> across nested word calls, return stack depth pressure
\ Edge case: Return stack pointer conflicts with LR save/restore
\ expect: 120

\ This stresses the interaction between >r/r> and the LR save mechanism

: square ( n -- n^2 )
  dup * ;

: cube ( n -- n^3 )
  dup dup * * ;

: save-comp ( n -- result )
  \ Save n, do unrelated work, restore
  >r
  42 square drop      \ some work with different stack
  r>                  \ get n back
;

: dbl-save ( a b -- a*2+b )
  \ Save both to return stack, operate, restore in order
  \ Stack: a b (b is TOS)
  >r                  \ R: b, Stack: a
  >r                  \ R: b a, Stack: empty
  100 drop            \ unrelated work
  r>                  \ R: b, Stack: a (first item pushed = last popped)
  2 *                 \ Stack: a*2
  r>                  \ R: empty, Stack: a*2 b
  +                   \ Stack: a*2+b
;

\ Verify return stack survives word calls
: word-a ( n -- n+10 )
  10 + ;

: word-b ( n -- n*2 )
  2 * ;

: cross-bnd ( n -- result )
  >r            \ save n to return stack
  5 word-a      \ call a word (15)
  word-b        \ call another word (30)
  drop          \ discard result
  r>            \ n should still be there!
;

: main
  \ Test 1: simple save across word call
  10 cross-bnd
  10 = if
    \ Test 2: double save
    \ dbl-save(7, 3) = 7*2 + 3 = 17
    7 3 dbl-save
    17 = if
      \ Test 3: save-comp
      25 save-comp
      25 = if
        \ All passed - return 120 as success marker
        120
      else 3 then
    else 2 then
  else 1 then
;
