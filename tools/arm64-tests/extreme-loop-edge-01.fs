\ expect: 42
\ Edge case: limit EQUALS start - zero iterations
\ Standard Forth: DO skips body when index >= limit
\ This is the exact boundary case: start == limit should NOT execute body
: main
  42
  5 5 do           \ limit=5, start=5, exactly equal
    drop 99        \ should NEVER execute
  loop
;
