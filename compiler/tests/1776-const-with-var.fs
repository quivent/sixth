\ expect: 15
3 constant INC
variable count
: main 0 count ! 5 0 do INC count +! loop count @ . cr ;
