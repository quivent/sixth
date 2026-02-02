\ expected: 1254400

20 constant N
create A N N * cells allot
create B N N * cells allot
create C N N * cells allot

: idx ( row col -- addr ) swap N * + cells ;

: init ( -- )
  N 0 do N 0 do
    i j + A i j idx + !
    j i - B i j idx + !
  loop loop ;

: dot ( row col -- sum )
  0 N 0 do
    2 pick i idx A + @
    2 pick i swap idx B + @
    * +
  loop nip nip ;

: mult ( -- )
  N 0 do N 0 do
    i j dot C i j idx + !
  loop loop ;

: sum ( -- n )
  0 N N * 0 do C i cells + @ + loop ;

: main init mult sum . cr ;
