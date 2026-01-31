\ expect: found 7
: search ( -- )
  1
  begin
    dup 7 = if dup . drop exit then
    1+
  again
;
: main s" found " type search cr ;
