\ Test 929: deep stack then mod (mod uses idiv, clobbers rdx:rax)
\ Stack: 7 8 9 17 5 -> mod -> 7 8 9 2
: main 7 8 9 17 5 mod . . . . cr ;
