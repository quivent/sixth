\ expected: 100000000
\ Dead code elimination - unused computation

: useless ( n -- n ) dup 1000 * drop ;

: main
  0 100000000 0 do
    i useless drop 1+
  loop . cr ;
