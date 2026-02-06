\ stress-words-05.fs - Word returning multiple values used by caller
\ expect: 29
\ split(n) -> (n/2, n mod 2)
\ combine(a,b) -> b*10 + a
\ process = split then combine
: split ( n -- n/2 n%2 ) dup 2 / swap 2 mod ;
: combine ( a b -- b*10+a ) 10 * + ;
: process ( n -- result ) split combine ;
: chain ( n -- result ) process process process ;
: main 99 chain ;
