\ expect: 42
\ category: sixth-word
\ reason: INTERPRET returns cleanly when source buffer exhausted
\ Test INTERPRET compiles and runs (source buffer is at EOF, returns immediately)
: main 42 . cr interpret ;
