\ expect: NOPQR
\ ROT13 one character at a time
: rot13 ( c -- c' )
  dup 65 >= over 90 <= and if  \ uppercase A-Z
    65 - 13 + 26 mod 65 +
    exit
  then ;
: main
  65 rot13 emit   \ A -> N
  66 rot13 emit   \ B -> O
  67 rot13 emit   \ C -> P
  68 rot13 emit   \ D -> Q
  69 rot13 emit   \ E -> R
  cr ;
