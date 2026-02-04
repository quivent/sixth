\ expected: 1000000000
\ Triple nested using I J, 1K x 1K x 1K
: main
  0
  1000 0 do
    1000 0 do
      1000 0 do
        j i + drop
        1+
      loop
    loop
  loop
  . cr
;
