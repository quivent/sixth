\ expect: 0 300
\ s>d ( n -- lo hi ), 100 s>d = ( 100 0 ), 200 s>d = ( 200 0 )
\ d+ gives ( 300 0 ), . . prints 0 then 300
: main
  100 s>d 200 s>d d+
  . . cr ;
