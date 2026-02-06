\ stress-words-06.fs - Forward reference chains
\ expect: 46
\ Forward refs: a->b->c->d->e
\ e=3, d=e+1=4, c=d*5=20, b=c+3=23, a=b*2=46
: a b 2 * ;
: b c 3 + ;
: c d 5 * ;
: d e 1 + ;
: e 3 ;
: main a ;
