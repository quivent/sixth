\ expect: 90
\ ADVERSARIAL: Combine move + fill in sequence
\ Fill memory with 'X', then move to new location, verify
: main
  here 8 allot       \ allocate src buffer (now at here)
  here 8 allot       \ allocate dst buffer (now at here)
  here 16 -          \ src addr (go back 16)
  dup 8 88 fill      \ fill src with 'X' (88)
  dup 8 + 8 move     \ move 8 bytes from src to dst
  here 8 - 2 + c@    \ read dst[2] - should be 88 = 'X'
  2 +                \ add 2 to get 90 = 'Z' (proves both ops worked)
;
