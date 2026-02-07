\ Adversarial: Multiple ?dup in sequence with mixed zero/nonzero
\ Tests that branch offsets are computed correctly when
\ multiple ?dup instructions are emitted in sequence
\ Also tests stack management across multiple conditionals
\ expect: 12
: main
  3 ?dup        \ 3 3 (duplicated)
  +             \ 6

  0 ?dup        \ 6 0 -> 6 0 (NOT duplicated since 0)
  +             \ 6

  2 ?dup        \ 6 2 2 (duplicated)
  +             \ 6 4
  +             \ 10

  0 ?dup        \ 10 0 (not duplicated)
  +             \ 10

  1 ?dup        \ 10 1 1 (duplicated)
  +             \ 10 2
  +             \ 12
;
