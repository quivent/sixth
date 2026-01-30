\ test_ctrl_21.fs - nested loops
: main : row 5 begin dup 0> while 42 emit 1- repeat drop ;: box 3 begin dup 0> while row cr 1- repeat drop ;box ;
