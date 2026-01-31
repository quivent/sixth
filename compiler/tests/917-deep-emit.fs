\ Test 917: deep stack then emit (syscall clobbers rcx, r11)
\ Stack: 1 2 3 4 65 -> emit prints 'A', stack: 1 2 3 4
: main 1 2 3 4 65 emit . . . . cr ;
