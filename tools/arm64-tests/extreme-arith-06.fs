\ expect: 1
\ Chained negates collapse: odd count = single negate
\ Tests that negate codegen is correct under repetition
: main -1 negate negate negate negate negate 255 and ;
