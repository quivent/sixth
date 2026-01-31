\ expect: 100
\ Test 987: verify (a+b)^2 = a^2 + 2ab + b^2 for a=7 b=3
\ (7+3)^2 = 100, 49 + 42 + 9 = 100
: sq dup * ;
: main 7 3 over over + sq rot rot 2dup * 2* rot sq + swap sq + = if 100 . else 0 . then cr ;
