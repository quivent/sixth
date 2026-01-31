\ expect: 24
\ tuck+ then add the two results
\ Stack: 10 7 -> tuck+ -> ( 7 17 ) -> + -> ( 24 )
: main 10 7 tuck+ + . cr ;
