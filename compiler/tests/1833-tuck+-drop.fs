\ expect: 9
\ tuck+ then drop TOS (sum) to keep old TOS (b)
\ Stack: 6 9 -> tuck+ -> ( 9 15 ) -> drop -> ( 9 )
: main 6 9 tuck+ drop . cr ;
