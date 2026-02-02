\ expect: 7 3
\ Pattern F03: 2>r 2r>
\ 2>r 2r> = identity — round-trip double-cell through return stack
: main 3 7 2>r 2r> . . cr ;
