\ Test 733: helper called in begin-until loop
: dec 1- ;
: main 5 begin dec dup 0= until . cr ;
