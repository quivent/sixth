\ expected: 255
\ String/memory ops stress - move and fill

4096 constant SIZE
create src SIZE allot
create dst SIZE allot

: main
  src SIZE 255 fill
  100000 0 do
    src dst SIZE move
    dst src SIZE move
  loop
  dst c@ . cr ;
