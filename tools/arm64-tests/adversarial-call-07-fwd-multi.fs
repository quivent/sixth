\ expect: 20
\ Multiple forward references to the same word
\ helper=5, use1=5, use2=10, use3=5+5+10=20
: use1 helper ;
: use2 helper helper + ;
: use3 helper use1 + use2 + ;
: helper 5 ;
: main use3 ;
