\ expect: 7
\ Test: multiple arithmetic ops in else branch → 7
: main 5 0 if 3 * 2 * else 3 + 1- then . cr ;
