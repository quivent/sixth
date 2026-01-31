\ expect: 123 0
\ um* then um/mod round-trip: 123 * 7 / 7 = 123, rem 0. TOS=quot first.
: main 123 7 um* 7 um/mod . . cr ;
