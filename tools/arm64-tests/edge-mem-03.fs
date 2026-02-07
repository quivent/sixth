\ expect: 42
\ EDGE: HERE manipulation with potential overflow concerns
\ Tests: Use here/allot in computation, push allot to upper limits of reasonable size
\ This tests that allot correctly advances here and values don't overflow

create marker 8 allot

: main
  \ Get initial here
  here

  \ Allot 2048 bytes (pushes here forward significantly)
  2048 allot

  \ Get new here
  here

  \ Compute difference (should be 2048)
  swap -

  \ Also store a marker to ensure memory is usable
  123 marker !

  \ Verify marker wasn't corrupted by allot
  marker @
  123 = if
    \ Difference should be 2048
    2048 = if
      42    \ success
    else
      1     \ wrong difference
    then
  else
    2       \ marker corrupted
  then
;
