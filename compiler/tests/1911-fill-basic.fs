\ expect: 42 42 42
create buf 8 allot
: main ( -- ) buf 3 42 fill buf c@ . buf 1 + c@ . buf 2 + c@ . cr ;
