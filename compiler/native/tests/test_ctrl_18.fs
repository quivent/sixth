\ test_ctrl_18.fs - power function
: main : power 1 swap begin dup 0> while rot over * -rot 1- repeat drop nip ;2 10 power . cr ;
