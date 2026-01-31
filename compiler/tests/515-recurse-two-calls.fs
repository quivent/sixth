\ Test 515: two recursive calls in one word (fibonacci pattern)
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 12 fib . cr ;
