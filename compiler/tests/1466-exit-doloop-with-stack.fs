\ expect: 77 99
: finder ( -- n ) 10 0 do i 2 = if 99 exit then loop 0 ;
: main 77 . finder . cr ;
