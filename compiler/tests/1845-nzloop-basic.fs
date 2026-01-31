\ expect: 3 2 1
\ nzloop: loops while TOS != 0, does NOT modify TOS
\ Body must print and decrement manually
: main 3 begin dup . 1- nzloop drop cr ;
