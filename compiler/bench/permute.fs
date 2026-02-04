\ expected: 3628800
\ Generate all permutations of 10 elements

create perm 10 allot
create used 10 allot
variable perm-count

: permute ( depth -- ) recursive
  dup 10 = if drop 1 perm-count +! exit then
  10 0 do
    i used + c@ 0= if
      i over perm + c!
      1 i used + c!
      dup 1+ recurse
      0 i used + c!
    then
  loop drop ;

: main
  10 0 do 0 i used + c! loop
  0 perm-count !
  0 permute
  perm-count @ . cr ;
