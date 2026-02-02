\ expect: 42
\ category: sixth-word
\ reason: tick returns xt usable by EXECUTE
\ Test ' returns xt that can be executed
: foo 42 . cr ;
: main ' foo execute ;
