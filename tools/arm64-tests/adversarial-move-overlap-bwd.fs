\ expect: BCDE
\ ADVERSARIAL: Overlapping regions (dst < src)
\ This tests forward copy when destination overlaps source from below
\ Pattern: [A][B][C][D][E] at positions 0-4
\          Copy positions 1-4 to positions 0-3
\          With forward copy: B->0, C->1, D->2, E->3
\          Result: [B][C][D][E][E]
: main
  here 10 allot
  here dup 65 swap c!         \ [A] at here
  1 + dup 66 swap c!          \ [B] at here+1
  1 + dup 67 swap c!          \ [C] at here+2
  1 + dup 68 swap c!          \ [D] at here+3
  1 + 69 swap c!              \ [E] at here+4
  drop
  \ Now: here contains "ABCDE"
  here 1 + here 4 move        \ copy "BCDE" to here (overlapping)
  here 4 type
;
