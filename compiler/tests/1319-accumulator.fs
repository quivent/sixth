\ expect: 55 385 10
variable sum
variable sum-sq
variable count
: accum ( n -- )
  1 count +!
  dup sum +!
  dup * sum-sq +! ;
: main
  0 sum !  0 sum-sq !  0 count !
  11 1 do i accum loop
  sum @ . sum-sq @ . count @ . cr ;
