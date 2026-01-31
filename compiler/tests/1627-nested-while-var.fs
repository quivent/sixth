\ expect: 6
\ Count total iterations: outer 0,1,2; inner runs outer+1 times each
\ outer=0: inner runs 1 time. outer=1: inner runs 2. outer=2: inner runs 3.
\ total = 1+2+3 = 6
variable total
variable outer
variable inner
: main
  0 total !
  0 outer !
  begin outer @ 3 < while
    0 inner !
    begin inner @ outer @ 1+ < while
      total @ 1+ total !
      inner @ 1+ inner !
    repeat
    outer @ 1+ outer !
  repeat
  total @ . cr ;
