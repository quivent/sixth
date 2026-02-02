\ expected: 8189
\ Ackermann function benchmark

: ack  \ WORKAROUND: stack comment breaks double-recurse with 3 items
  over 0= if
    nip 1+
  else
    dup 0= if
      drop 1- 1 recurse
    else
      over 1- -rot 1- recurse recurse
    then
  then
;

: main
  3 10 ack . cr
;
