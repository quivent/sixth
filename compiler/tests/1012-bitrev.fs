\ expect: 128
: bitrev8 dup 1 and 128 * swap dup 2 / 1 and 64 * rot + swap dup 4 / 1 and 32 * rot + swap 8 / 1 and 16 * + ;
: main 1 bitrev8 . cr ;
