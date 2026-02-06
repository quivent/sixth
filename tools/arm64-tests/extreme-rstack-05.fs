\ expect: 77
\ Test: Rapid push/pop cycling 200 times
: cycle ( n count -- n )
  0 do >r r> loop
;
: main 77 200 cycle ;
