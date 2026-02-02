\ expect: 56
3 constant W
4 constant H
2 constant BORDER
: area ( w h -- n ) * ;
: with-border ( -- n ) W BORDER 2 * + H BORDER 2 * + area ;
: main with-border . cr ;
