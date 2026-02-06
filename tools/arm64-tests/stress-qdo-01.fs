\ Stress ?DO test 01: zero iterations (start = limit)
\ ?DO should skip body entirely when index >= limit
\ Unlike DO, ?DO checks BEFORE pushing to return stack
\ expect: 42
: main 42 5 5 ?do 1 + loop ;
