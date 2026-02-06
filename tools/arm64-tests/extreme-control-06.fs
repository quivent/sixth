\ expect: 82
\ Test: Asymmetric ELSE branches - one heavy, one empty
\ Tests branch offset calculation with mismatched sizes

: main
  1 if
    1 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 + 1 + 1 + 1 +
    1 + 1 +
  else
  then
;
