\ expect: 0 even 1 odd 2 even 3 odd 4 even
: main
  5 0 do
    i .
    i 2 mod 0= if
      s" even" type
    else
      s" odd" type
    then
    32 emit
  loop
  cr
;
