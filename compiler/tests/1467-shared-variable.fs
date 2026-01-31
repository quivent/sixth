\ expect: 15
variable acc
: add-to ( n -- ) acc @ + acc ! ;
: get-acc ( -- n ) acc @ ;
: main 0 acc ! 5 add-to 10 add-to get-acc . cr ;
