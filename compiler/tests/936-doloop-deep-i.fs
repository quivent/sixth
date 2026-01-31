\ Test 936: values on stack, do/loop with i, verify stack after
\ 3 values below, loop prints i=0,1,2, then print 3 values
: main 10 20 30 3 0 do i . loop . . . cr ;
