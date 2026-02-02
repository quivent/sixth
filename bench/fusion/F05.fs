\ expect: SKIP
\ Pattern F05: -rot rot (UNIMPLEMENTED — -rot not in compiler)
\ -rot rot = identity — reverse triple rotation and back
\ SKIP: -rot is not implemented in sixth.fs
: main 3 5 7 -rot rot . . . cr ;
