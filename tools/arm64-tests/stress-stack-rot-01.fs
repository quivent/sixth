\ stress-stack-rot-01.fs - rot/-rot sequences that should restore original order
\ Tests: rot rot rot = identity, rot -rot = identity, complex sequences
\ Edge case: Memory load/store ordering, register preservation
\ expect: 99

\ Mathematical properties:
\ - rot rot rot = identity (three rotations = back to start)
\ - rot -rot = identity (forward then back)
\ - -rot rot = identity (back then forward)
\ - -rot -rot -rot = identity (three reverse = back to start)

: t-3rot ( -- flag )
  \ rot rot rot should leave stack unchanged
  11 22 33         \ 11 22 33
  rot rot rot      \ should be 11 22 33 again
  33 = if
    22 = if
      11 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-rot-unrot ( -- flag )
  \ rot followed by -rot should be identity
  44 55 66         \ 44 55 66
  rot -rot         \ should be 44 55 66
  66 = if
    55 = if
      44 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-unrot-rot ( -- flag )
  \ -rot followed by rot should be identity
  77 88 99         \ 77 88 99
  -rot rot         \ should be 77 88 99
  99 = if
    88 = if
      77 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-3unrot ( -- flag )
  \ -rot -rot -rot should leave stack unchanged
  12 34 56         \ 12 34 56
  -rot -rot -rot   \ should be 12 34 56
  56 = if
    34 = if
      12 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-1rot ( -- flag )
  \ Single rot: ( x y z -- y z x )
  1 2 3            \ 1 2 3 (1 at bottom)
  rot              \ 2 3 1
  1 = if           \ TOS is 1
    3 = if
      2 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-1unrot ( -- flag )
  \ Single -rot: ( x y z -- z x y )
  1 2 3            \ 1 2 3
  -rot             \ 3 1 2
  2 = if           \ TOS is 2
    1 = if
      3 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-complex ( -- flag )
  \ A complex sequence that should return to original
  \ rot rot = two thirds around = same as -rot
  \ So: rot rot -rot = rot
  5 6 7            \ 5 6 7
  rot rot -rot     \ same as rot: 6 7 5
  5 = if
    7 = if
      6 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: t-swap-rot ( -- flag )
  \ Test rot combined with swap
  1 2 3            \ 1 2 3
  rot              \ 2 3 1
  swap             \ 2 1 3
  rot              \ 1 3 2
  swap             \ 1 2 3 (back to original!)
  3 = if
    2 = if
      1 = if
        1
      else 0 then
    else 0 then
  else 0 then
;

: main
  t-1rot
  1 = if
    t-1unrot
    1 = if
      t-3rot
      1 = if
        t-rot-unrot
        1 = if
          t-unrot-rot
          1 = if
            t-3unrot
            1 = if
              t-complex
              1 = if
                t-swap-rot
                1 = if
                  99    \ All tests passed!
                else 8 then
              else 7 then
            else 6 then
          else 5 then
        else 4 then
      else 3 then
    else 2 then
  else 1 then
;
