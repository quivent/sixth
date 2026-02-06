\ Adversarial test: Mixed control with early EXIT
\ Tests exit from word with nested IF
\ expect: 25

: finder ( n -- result )
  dup 5 = if
    drop 25 exit
  then
  dup 3 = if
    drop 15 exit
  then
  drop 0
;

: main
  5 finder    \ should return 25
;
