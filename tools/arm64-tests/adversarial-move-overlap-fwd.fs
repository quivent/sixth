\ expect: ABABA
\ ADVERSARIAL: Overlapping regions (dst > src)
\ This tests forward copy when destination overlaps source from above
\ Pattern: [A][B][C][D][E] at positions 0-4
\          Copy positions 0-2 to positions 2-4
\ Since move copies forward (low to high):
\   Copy A to pos 2: [A][B][A][D][E]
\   Copy B to pos 3: [A][B][A][B][E]
\   Copy A (now at pos 2) to pos 4: [A][B][A][B][A]
\ Result: "ABABA"
: main
  s" ABCDE" drop here 5 move  \ copy to here first
  here here 2 + 3 move        \ copy from here to here+2 (overlapping)
  here 5 type
;
