\ expect: 100
\ This is the common idiom: ?dup if <use-it> then
\ Tests that CBZ in ?dup doesn't corrupt flags used by subsequent if
\ expect: 100
: main
  0             \ accumulator

  \ Pattern 1: non-zero value
  \ 42 ?dup -> 42 42
  \ if -> consumes 42 (true), leaves: acc 42
  \ + -> acc+42
  42 ?dup if + then
  \ Stack: 42

  \ Pattern 2: zero value - ?dup should NOT duplicate
  \ 0 ?dup -> 0 (not duplicated)
  \ if -> consumes 0 (false), skips body
  0 ?dup if 999 + then
  \ Stack: 42

  \ Pattern 3: another non-zero
  \ 10 ?dup -> 10 10
  \ if -> consumes 10 (true), leaves 10
  \ + -> 42+10 = 52
  10 ?dup if + then
  \ Stack: 52

  \ Pattern 4: Nested ?dup if
  \ 6 ?dup -> 6 6
  \ if -> consumes 6 (true), leaves 6
  \   6 ?dup -> 6 6
  \   if -> consumes 6 (true), leaves 6
  \     + -> 52+6 = 58
  6 ?dup if ?dup if + then then
  \ Stack: 58

  \ Pattern 5: zero at second level
  \ 8 ?dup -> 8 8
  \ if -> consumes 8 (true), leaves 8
  \   drop -> stack is just 58
  \   0 ?dup -> 0 (not duplicated)
  \   if -> false, skip
  8 ?dup if drop 0 ?dup if 888 + then then
  \ Stack: 58

  42 +          \ 58 + 42 = 100
;
