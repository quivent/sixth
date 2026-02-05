\ expect: 16
: inc 1 + ;
: inc2 inc inc ;
: inc4 inc2 inc2 ;
: inc8 inc4 inc4 ;
: main 8 inc8 ;
