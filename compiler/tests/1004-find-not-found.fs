\ expect: 0
\ category: sixth-word
\ reason: FIND returns 0 when word not in dictionary
\ Test find returns 0 for word not in dictionary
: foo 42 ;
: main s" xyz" find . drop cr ;
