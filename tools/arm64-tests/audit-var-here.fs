\ expect: 8
\ Test that here starts after all variables
\ Variable x at offset 8, here at offset 16, difference = 8
variable x
: main here x - ;
