\ expect: 243
\ Recursive power: 3^5
: rpow ( base exp -- result )
  dup 0= if drop drop 1 exit then
  dup 1 = if drop exit then
  over swap 1- rpow * ;
: main 3 5 rpow . cr ;
