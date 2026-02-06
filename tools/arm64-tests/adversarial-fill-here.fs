\ expect: 1
\ ADVERSARIAL: Fill at current here pointer
\ Tests fill doesn't corrupt the here pointer itself
\ Verifies memory after fill is what we expect

: main
  here dup          \ save original here
  16 allot          \ allocate space
  here 16 - swap    \ addr original-here
  drop              \ addr

  dup 16 42 fill    \ fill with 42

  \ Verify fill worked
  dup c@ 42 =
  over 8 + c@ 42 = and
  over 15 + c@ 42 = and

  \ Verify here still works (can allot more)
  here swap drop
  4 allot
  here swap - 4 = and

  if 1 else 0 then
;
