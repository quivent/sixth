\ expect: -1 -3
\ 7 s>d + (-10) s>d = 7 + (-10) = -3 as double
\ 7 s>d = ( 7 0 ), -10 s>d = ( -10 -1 )
\ d+: lo = 7 + (-10) = -3, hi = 0 + (-1) + carry
\ Actually d+ on ( 7 0 -10 -1 ): lo = 7 + (-10), hi = 0 + (-1) + carry
\ 7 + (-10) unsigned: 7 + 0xFFFF...FFF6 = 0xFFFF...FFFD = -3, no carry
\ hi = 0 + (-1) + 0 = -1
: main 7 s>d -10 s>d d+ . . cr ;
