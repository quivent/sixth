\ expect: -4 -1
\ -13 / 3: symmetric gives quot=-4, rem=-1 (rem has sign of dividend)
\ -13 as double: ( -13 -1 )
: main -13 -1 3 sm/rem . . cr ;
