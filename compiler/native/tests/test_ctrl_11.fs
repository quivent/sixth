\ test_ctrl_11.fs - sum 1 to n
: main : sum 0 swap begin dup 0> while tuck + swap 1- repeat drop ;10 sum . cr ;
