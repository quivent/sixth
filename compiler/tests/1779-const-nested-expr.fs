\ expect: 56
constant W 3
constant H 4
constant BORDER 2
: area ( w h -- n ) * ;
: with-border ( -- n ) W BORDER 2 * + H BORDER 2 * + area ;
: main with-border . cr ;
