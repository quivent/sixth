\ expect: 100
\ Extreme Test 08: Diamond call pattern - multiple paths to same word
\ Tests: code sharing, word address consistency

: bottom 10 ;
: left bottom 2 * ;
: right bottom 3 * ;
: top left right + ;

: dleft top ;
: dright top ;
: dtop dleft dright + ;

: main dtop ;
