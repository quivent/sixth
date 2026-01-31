\ Test 732: min helper word
: mymin 2dup < if drop else nip then ;
: main 3 7 mymin . cr ;
