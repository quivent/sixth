\ expect: three
: main
  3
  dup 1 = if drop s" one" type else
  dup 2 = if drop s" two" type else
  dup 3 = if drop s" three" type else
  drop s" other" type
  then then then
  cr
;
