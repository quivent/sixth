\ Test 361: deep call chain
: a 1+ ;
: b a a ;
: c b b ;
: d c c ;
: main 0 d . cr ;
