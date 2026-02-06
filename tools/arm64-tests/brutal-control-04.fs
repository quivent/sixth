\ expect: 0
\ Test: LEAVE from nested loops

variable found

: find-sum ( target -- flag )
  \ Search for i+j = target where 0 <= i,j < 10
  0 found !
  10 0 do
    10 0 do
      i j + over = if
        1 found !
        leave               \ exit inner loop
      then
    loop
    found @ if leave then   \ if found, exit outer loop
  loop
  drop found @
;

: main
  0 find-sum 1 <> if 1 then     \ 0+0=0
  18 find-sum 1 <> if 2 then    \ 9+9=18
  5 find-sum 1 <> if 3 then     \ multiple ways
  20 find-sum 0 <> if 4 then    \ impossible (max is 18)
  0
;
