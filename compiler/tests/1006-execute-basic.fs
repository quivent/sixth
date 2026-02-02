\ expect: 42
\ category: sixth-word
\ reason: EXECUTE calls xt returned by FIND
\ Test execute calls xt
: foo 42 . cr ;
: main s" foo" find drop execute ;
