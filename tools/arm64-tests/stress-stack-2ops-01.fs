\ stress-stack-2ops-01.fs - 2dup/2drop with odd stack depths
\ Tests: 2dup and 2drop when stack has odd number of items
\ Edge case: Pair operations on non-pair-aligned stack depths
\ expect: 77

\ 2dup duplicates top TWO items: ( a b -- a b a b )
\ 2drop discards top TWO items: ( a b -- )
\ What happens with odd depths? Does it corrupt?

: t-2dup-odd3 ( -- flag )
  \ Start with 3 items (odd), do 2dup
  1 2 3          \ depth 3: [1] [2] [3] (1 bottom, 3 TOS)
  2dup           \ depth 5: [1] [2] [3] [2] [3]
  \ After 2dup: TOS=3, NOS=2, 3rd=3, 4th=2, 5th=1
  3 = if         \ check TOS=3
    2 = if       \ check NOS=2 (copied from original NOS)
      3 = if     \ check 3rd=3 (original TOS)
        2 = if   \ check 4th=2 (original NOS)
          1 = if \ check 5th=1 (original 3rd)
            1    \ success
          else 0 then
        else 0 then
      else 0 then
    else 0 then
  else 0 then
;

: t-2drop-odd5 ( -- flag )
  \ Start with 5 items (odd), do 2drop
  10 20 30 40 50    \ depth 5
  2drop             \ should leave 10 20 30
  30 = if
    20 = if
      10 = if
        1         \ success
      else 0 then
    else 0 then
  else 0 then
;

: t-2dup-pair ( -- flag )
  \ Test with exactly 2 items (the normal case)
  42 99            \ depth 2: [42] [99]
  2dup             \ depth 4: [42] [99] [42] [99]
  99 = if          \ TOS=99
    42 = if        \ NOS=42
      99 = if      \ 3rd=99
        42 = if    \ 4th=42
          1       \ success
        else 0 then
      else 0 then
    else 0 then
  else 0 then
;

: t-chain-2ops ( -- flag )
  \ Chain multiple 2dup/2drop operations
  5 6              \ depth 2
  2dup 2dup 2dup   \ depth 8: 5 6 5 6 5 6 5 6
  2drop 2drop      \ depth 4: 5 6 5 6
  +                \ depth 3: 5 6 11
  +                \ depth 2: 5 17
  +                \ depth 1: 22
  22 = if 1 else 0 then
;

: t-2dup-after ( -- flag )
  \ 2dup after other stack manipulations
  10 20 30         \ 10 20 30
  rot              \ 20 30 10
  2dup             \ 20 30 10 30 10
  + +              \ 20 30 50
  + +              \ 100
  100 = if 1 else 0 then
;

: main
  t-2dup-odd3
  1 = if
    t-2drop-odd5
    1 = if
      t-2dup-pair
      1 = if
        t-chain-2ops
        1 = if
          t-2dup-after
          1 = if
            77    \ All tests passed!
          else 5 then
        else 4 then
      else 3 then
    else 2 then
  else 1 then
;
