\ expect: 0
\ Test close-file syscall works (doesn't crash)
\ Close invalid fd -1, drop result, return 0
: main -1 close-file drop 0 ;
