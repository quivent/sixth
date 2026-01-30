\ test_ctrl_15.fs - fibonacci
: main : fib dup 2 < if drop 1 else dup 1- fib swap 2 - fib + then ;10 fib . cr ;
