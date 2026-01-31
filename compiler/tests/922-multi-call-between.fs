\ Test 922: multiple function calls with values on stack between calls
: inc 1+ ;
: main 10 inc 20 inc 30 inc . . . cr ;
