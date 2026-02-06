\ expect: 35
\ STRESS: CREATE buffer with recursive access pattern
\ Tests: Recursive word accessing buffer at different indices

create table 40 allot   \ 5 cells

: table! ( n i -- )  8 * table + ! ;
: table@ ( i -- n )  8 * table + @ ;

: recursive-sum ( i -- n )
  dup 0< if drop 0 exit then
  dup table@
  swap 1- recursive-sum
  + ;

: main
  5 0 table!
  10 1 table!
  15 2 table!
  20 3 table!
  25 4 table!
  \ Sum indices 0-4: 5+10+15+20+25 = 75
  \ But we want 35, so sum indices 0-2: 5+10+15 = 30, add 5 = 35
  2 recursive-sum
  0 table@ +     \ 30 + 5 = 35
;
