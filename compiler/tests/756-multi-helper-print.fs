\ Test 756: multiple helpers each printing
: show-a 65 emit ;
: show-b 66 emit ;
: show-c 67 emit ;
: main show-a show-b show-c cr ;
