\ expect: deep
: check ( n -- )
  dup 0> if
    dup 10 < if
      dup 5 = if
        drop s" deep" type exit
      then
    then
  then
  drop s" none" type
;
: main 5 check cr ;
