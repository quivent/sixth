\ expect: 100
\ Test: Interleaved IF and BEGIN with alternating true/false paths
\ Exercises cf-stack push/pop ordering with mixed construct types

: zigzag ( n -- result )
  dup 0= if
    drop 100
  else
    begin
      dup 1 >
    while
      1-
      dup 2 mod 0= if
        dup
      else
        0
      then
      drop
    repeat
    drop 50
  then
;

: main
  0 zigzag     \ should return 100 (0= case)
;
