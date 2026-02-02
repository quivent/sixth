\ expect: 42
\ Pattern F01: dup drop
\ dup drop = identity — value must survive unchanged
: main 42 dup drop . cr ;
