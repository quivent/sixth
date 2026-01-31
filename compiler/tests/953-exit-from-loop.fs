\ expect: 5
\ Test 953: return value from inside a loop via exit
: find-first 10 0 do i 5 = if i . cr exit then loop ;
: main find-first ;
