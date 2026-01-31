\ expect: 42
\ Test 850: do loop finding max
: main 0 8 0 do i dup 5 * 7 + 100 mod dup rot > if swap drop else drop then loop . cr ;
