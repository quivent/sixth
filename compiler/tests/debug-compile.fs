\ debug-compile.fs - Test compile-time builtin recognition
\ expect: 15

: main
  5 3 * .    \ uses builtins *, .
;
