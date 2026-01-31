\ Test 381: sum of squares via nested calls
: sq dup * ;
: sumsq ( a b -- n ) sq swap sq + ;
: main 3 4 sumsq . cr ;
