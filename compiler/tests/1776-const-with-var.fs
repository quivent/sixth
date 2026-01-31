\ expect: 15
constant INC 3
variable count
: main 0 count ! 5 0 do INC count +! loop count @ . cr ;
