\ expect: 0
\ Test: MIN-INT and MAX-INT edge cases for signed comparison
\ On 64-bit: MIN-INT = $8000000000000000, MAX-INT = $7FFFFFFFFFFFFFFF

: min-int  1 63 lshift ;                    \ $8000000000000000
: max-int  min-int 1- ;                     \ $7FFFFFFFFFFFFFFF

: main
  min-int 0 < -1 <> if 1 exit then          \ MIN-INT < 0 (signed)
  max-int 0 > -1 <> if 2 exit then          \ MAX-INT > 0 (signed)
  min-int max-int < -1 <> if 3 exit then    \ MIN-INT < MAX-INT
  max-int min-int > -1 <> if 4 exit then    \ MAX-INT > MIN-INT

  \ MIN-INT is the most negative number
  min-int -1 < -1 <> if 5 exit then         \ MIN-INT < -1
  max-int 1 > -1 <> if 6 exit then          \ MAX-INT > 1

  \ Edge: MAX-INT + 1 wraps to MIN-INT (not tested here, just verify comparison)
  min-int min-int = -1 <> if 7 exit then    \ MIN-INT = MIN-INT
  max-int max-int = -1 <> if 8 exit then    \ MAX-INT = MAX-INT

  0
;
