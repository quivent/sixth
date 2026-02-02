\ Test 2202: PARSE compiles and runs
\ Input buffer is empty at runtime, so PARSE returns 0 length
\ expect: 0
: main
  32 parse nip . cr ;
