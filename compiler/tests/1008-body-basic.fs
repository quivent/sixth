\ expect: 42
\ category: sixth-word
\ reason: >BODY returns data field address from CREATE'd word
\ Test >body extracts data address from CREATE'd word
create foo 42 ,
: main ' foo >body @ . cr ;
