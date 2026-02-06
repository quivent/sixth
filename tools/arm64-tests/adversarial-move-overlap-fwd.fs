\ expect: ABABC
\ ADVERSARIAL: Overlapping regions (dst > src)
\ This tests backward copy when destination overlaps source from above
\ Pattern: [A][B][C][D][E] at positions 0-4
\          Copy positions 0-2 to positions 2-4
\ Since move copies BACKWARD when dst > src (to preserve source data):
\   Copy C (src[2]) to pos 4: [A][B][C][D][C]
\   Copy B (src[1]) to pos 3: [A][B][C][B][C]
\   Copy A (src[0]) to pos 2: [A][B][A][B][C]
\ Result: "ABABC" (correct overlap handling)
: main
  s" ABCDE" drop here 5 move  \ copy to here first
  here here 2 + 3 move        \ copy from here to here+2 (overlapping)
  here 5 type
;
