\ expect: 99
\ Test: Helper word does >r r> - ensures return address not corrupted
: save-restore ( n -- n ) >r r> ;
: main 99 save-restore save-restore save-restore save-restore save-restore ;
