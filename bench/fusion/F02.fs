\ expect: 42
\ Pattern F02: >r r>
\ >r r> = identity — round-trip through return stack
: main 42 >r r> . cr ;
