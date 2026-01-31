\ expect: 55 44 33 22 11
\ Test 918: deep stack then . (print clobbers many regs)
\ Stack: 11 22 33 44 55 -> print 55, then 44 33 22 11
: main 11 22 33 44 55 . . . . . cr ;
