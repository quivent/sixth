\ expect: 99
\ EDGE: Word call across memory operations
\ Tests: Memory ops work correctly when split across word calls
\ This tests that memory state is preserved across function calls

create shared 8 allot

: store-val ( n -- )
  shared !
;

: load-val ( -- n )
  shared @
;

: double-it ( -- )
  load-val 2 * store-val
;

: main
  \ Store initial value via helper
  12 store-val

  \ Double it a few times via helper
  double-it   \ 12 * 2 = 24
  double-it   \ 24 * 2 = 48

  \ Add a bit more and store directly
  load-val 3 + shared !   \ 48 + 3 = 51

  \ Double once more
  double-it   \ 51 * 2 = 102

  \ Final load and adjust
  load-val 3 -   \ 102 - 3 = 99
;
