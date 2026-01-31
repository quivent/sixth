\ expect: 1 odd 2 even 3 odd 4 even 5 odd
: main
  1
  begin
    dup .
    dup 2 mod 0= if
      s" even" type
    else
      s" odd" type
    then
    32 emit
    1+
  dup 6 = until
  drop
  cr
;
