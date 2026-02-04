\ expected: 262143
\ Build tree recursively, count nodes (depth 18 = 262143 nodes)

: treebuild ( depth -- count ) recursive
  dup 0= if drop 1 exit then
  1- dup recurse swap recurse + 1+ ;

: main
  17 treebuild . cr ;
