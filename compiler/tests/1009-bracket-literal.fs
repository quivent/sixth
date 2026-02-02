\ expect: 42
\ category: sixth-word
\ reason: [ ] LITERAL embeds compile-time value in definition
\ Test [ ] literal - embed constant at compile time
: foo [ 42 ] literal . cr ;
: main foo ;
