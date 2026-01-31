\ expect: 6 6 6
variable acc
: main
  3 0 do
    0 acc !
    4 0 do i acc +! loop
    acc @ .
  loop cr ;
