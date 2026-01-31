\ expect: 6 21
variable sum
: main
  0 sum !
  0
  begin
    dup 10 < sum @ 20 < and
  while
    dup 1+ sum +!
    1+
  repeat
  . sum @ . cr ;
