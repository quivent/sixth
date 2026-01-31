\ Test 923: rot with 5 values on stack (only affects top 3)
\ Stack: 1 2 3 4 5 -> rot -> 1 2 4 5 3
: main 1 2 3 4 5 rot . . . . . cr ;
