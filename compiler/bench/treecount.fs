\ expected: 262143
\ Count nodes recursively, simulate 100K node tree (depth 17)

: treecount ( depth -- count ) recursive
  dup 0= if drop 1 exit then
  1- dup recurse swap recurse + 1+ ;

: main
  1000 0 do 17 treecount drop loop
  17 treecount . cr ;
