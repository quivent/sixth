\ expect: 1 2 3 4 5
: count-to-5 ( -- )
  1
  begin
    dup .
    dup 5 = if drop exit then
    1+
  again
;
: main count-to-5 cr ;
