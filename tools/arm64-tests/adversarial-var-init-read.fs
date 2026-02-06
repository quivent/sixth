\ expect: 5
\ Variable must be stored before read - test store then read
variable x
: main 5 x ! x @ ;
