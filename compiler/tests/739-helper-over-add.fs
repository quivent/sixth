\ expect: 7 3
\ Test 739: helper using over
: add-keep ( a b -- a a+b ) over + ;
: main 3 4 add-keep . . cr ;
