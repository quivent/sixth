\ expected: 1073741824
\ Recursive power, 10M calls

: power ( base exp -- result ) recursive
  dup 0= if 2drop 1 exit then
  dup 1 and if
    1- over swap recurse *
  else
    2/ over swap recurse dup *
  then nip ;

: main
  10000000 0 do 2 30 power drop loop
  2 30 power . cr ;
