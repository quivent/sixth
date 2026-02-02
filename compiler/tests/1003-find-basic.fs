\ expect: -1
\ category: sixth-word
\ reason: FIND returns -1 for non-immediate word
\ Test find returns -1 for normal word
: foo 42 ;
: main s" foo" find . drop cr ;
