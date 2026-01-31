\ expect: 42 99
create buf1 8 allot
create buf2 8 allot
: main 42 buf1 ! 99 buf2 ! buf1 @ . buf2 @ . cr ;
