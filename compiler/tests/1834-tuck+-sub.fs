\ expect: -4
\ tuck+ then subtract: ( b a+b ) - -> b-(a+b) = -a
\ Stack: 4 3 -> tuck+ -> ( 3 7 ) -> - -> 3-7 = -4
: main 4 3 tuck+ - . cr ;
