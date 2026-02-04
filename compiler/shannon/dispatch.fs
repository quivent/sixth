\ compiler/shannon/dispatch.fs - Builtin Word Dispatch Table
\ Part of Shannon Architecture refactoring
\
\ This module provides a data-driven dispatch table for builtin words,
\ replacing the 700-line string comparison cascade in compile-builtin.
\
\ The table is DATA. Adding a new builtin is one line. Lookup is a
\ simple loop over fixed-size entries.
\
\ Usage:
\   s" dup" find-builtin if ( xt flags ) ... then

\ ============================================================
\ FLAG BITS
\ ============================================================
\ These describe optimization properties of each builtin.

1 constant F_FOLD1      \ foldable with 1 constant arg
2 constant F_FOLD2      \ foldable with 2 constant args
4 constant F_COMMUT     \ commutative (swap can be absorbed)
8 constant F_STACKOP    \ pure stack manipulation
16 constant F_IO        \ has I/O side effects
32 constant F_CONTROL   \ control flow (special handling)
64 constant F_MEMORY    \ memory access
128 constant F_RSTACK   \ return stack manipulation

\ Combined flags for common patterns
F_FOLD2 F_COMMUT or constant F_FOLD2C   \ foldable binary, commutative
F_FOLD2 constant F_FOLD2N               \ foldable binary, non-commutative

\ ============================================================
\ TABLE STRUCTURE
\ ============================================================
\ Each entry is 24 bytes:
\   0-15:  name (16 bytes, null-padded, max 15 chars)
\   16-19: flags (4 bytes)
\   20-23: reserved/padding
\
\ The table is terminated by an entry with name[0] = 0.
\
\ Note: xt (execution token) is NOT stored in the table.
\ The dispatcher returns the name match; the caller looks up
\ or dispatches based on flags. This keeps the table pure data.

24 constant ENTRY-SIZE
16 constant NAME-SIZE

\ ============================================================
\ TABLE BUILDING HELPERS
\ ============================================================

variable table-here

: entry, ( flags addr u -- )
  \ Add one entry: name string (addr u) and flags
  \ Pads name to 16 bytes, adds flags, pads to 24 bytes
  table-here @ >r            \ R: entry
  \ Zero the entry first
  r@ 24 0 fill               \ fill(entry, 24, 0)
  \ Copy name (up to 15 chars): ( flags addr u )
  dup 15 > if drop 15 then   \ truncate ( flags addr u' )
  r@ swap                    \ ( flags addr entry u' )
  move                       \ move(src=addr, dest=entry, u') leaves ( flags )
  \ Store flags at offset 16
  r> 16 + !                  \ store flags, R empty
  \ Advance table pointer
  table-here @ 24 + table-here ! ;

: end-table ( -- )
  \ Terminate table with null entry
  table-here @ 24 0 fill
  table-here @ 24 + table-here ! ;

\ ============================================================
\ TABLE DATA
\ ============================================================

create builtin-table 4096 allot
builtin-table table-here !

\ ---- Stack manipulation ----
F_STACKOP s" dup"         entry,
F_STACKOP s" drop"        entry,
F_STACKOP s" swap"        entry,
F_STACKOP s" over"        entry,
F_STACKOP s" rot"         entry,
F_STACKOP s" nip"         entry,
F_STACKOP s" tuck"        entry,
F_STACKOP s" 2dup"        entry,
F_STACKOP s" 2drop"       entry,
F_STACKOP s" 2swap"       entry,
F_STACKOP s" 2over"       entry,
F_STACKOP s" -rot"        entry,
F_STACKOP s" ?dup"        entry,
F_STACKOP s" depth"       entry,
F_STACKOP s" pick"        entry,
F_STACKOP s" dup2"        entry,

\ ---- Binary arithmetic (commutative, foldable) ----
F_FOLD2C s" +"            entry,
F_FOLD2C s" *"            entry,
F_FOLD2C s" and"          entry,
F_FOLD2C s" or"           entry,
F_FOLD2C s" xor"          entry,
F_FOLD2C s" min"          entry,
F_FOLD2C s" max"          entry,
F_FOLD2C s" ="            entry,
F_FOLD2C s" <>"           entry,
F_FOLD2C s" d+"           entry,

\ ---- Binary arithmetic (non-commutative, foldable) ----
F_FOLD2N s" -"            entry,
F_FOLD2N s" /"            entry,
F_FOLD2N s" mod"          entry,
F_FOLD2N s" /mod"         entry,
F_FOLD2N s" */mod"        entry,
F_FOLD2N s" */"           entry,
F_FOLD2N s" lshift"       entry,
F_FOLD2N s" rshift"       entry,
F_FOLD2N s" <"            entry,
F_FOLD2N s" >"            entry,
F_FOLD2N s" <="           entry,
F_FOLD2N s" >="           entry,
F_FOLD2N s" u<"           entry,
F_FOLD2N s" within"       entry,
F_FOLD2N s" um*"          entry,
F_FOLD2N s" m*"           entry,
F_FOLD2N s" um/mod"       entry,
F_FOLD2N s" sm/rem"       entry,
F_FOLD2N s" fm/mod"       entry,
F_FOLD2N s" d-"           entry,

\ ---- Unary arithmetic (foldable) ----
F_FOLD1 s" negate"        entry,
F_FOLD1 s" invert"        entry,
F_FOLD1 s" abs"           entry,
F_FOLD1 s" 1+"            entry,
F_FOLD1 s" 1-"            entry,
F_FOLD1 s" 2*"            entry,
F_FOLD1 s" 2/"            entry,
F_FOLD1 s" 2+"            entry,
F_FOLD1 s" 2-"            entry,
F_FOLD1 s" cells"         entry,
F_FOLD1 s" cell+"         entry,
F_FOLD1 s" chars"         entry,
F_FOLD1 s" char+"         entry,
F_FOLD1 s" 0="            entry,
F_FOLD1 s" 0<"            entry,
F_FOLD1 s" 0>"            entry,
F_FOLD1 s" 0 <>"           entry,
F_FOLD1 s" s>d"           entry,
F_FOLD1 s" >body"         entry,
F_FOLD1 s" count"         entry,
F_FOLD1 s" nos+"          entry,
F_FOLD1 s" tuck+"         entry,

\ ---- Constants (push immediate value) ----
F_FOLD1 s" bl"            entry,
F_FOLD1 s" true"          entry,
F_FOLD1 s" false"         entry,
F_FOLD1 s" r/o"           entry,
F_FOLD1 s" w/o"           entry,
F_FOLD1 s" r/w"           entry,

\ ---- Memory ----
F_MEMORY s" @"            entry,
F_MEMORY s" !"            entry,
F_MEMORY s" c@"           entry,
F_MEMORY s" c!"           entry,
F_MEMORY s" +!"           entry,
F_MEMORY s" move"         entry,
F_MEMORY s" fill"         entry,
F_MEMORY s" /string"      entry,
F_MEMORY s" base"         entry,
F_MEMORY s" >in"          entry,
F_MEMORY s" here"         entry,
F_MEMORY s" ,"            entry,
F_MEMORY s" c,"           entry,
F_MEMORY s" s,"           entry,

\ ---- I/O (has side effects) ----
F_IO s" ."                entry,
F_IO s" cr"               entry,
F_IO s" emit"             entry,
F_IO s" type"             entry,
F_IO s" space"            entry,
F_IO s" spaces"           entry,
F_IO s" u."               entry,
F_IO s" key"              entry,
F_IO s" <#"               entry,
F_IO s" hold"             entry,
F_IO s" sign"             entry,
F_IO s" #"                entry,
F_IO s" #s"               entry,
F_IO s" #>"               entry,
F_IO s" decimal"          entry,
F_IO s" source"           entry,
F_IO s" parse"            entry,
F_IO s" word"             entry,
F_IO s" accept"           entry,
F_IO s" refill"           entry,
F_IO s" find"             entry,
F_IO s" '"                entry,
F_IO s" interpret"        entry,
F_IO s" evaluate"         entry,
F_IO s" open-file"        entry,
F_IO s" create-file"      entry,
F_IO s" close-file"       entry,
F_IO s" read-file"        entry,
F_IO s" write-file"       entry,
F_IO s" slurp-file"       entry,
F_IO s" include"          entry,
F_IO s" argc"             entry,
F_IO s" argv"             entry,

\ ---- Return stack ----
F_RSTACK s" >r"           entry,
F_RSTACK s" r>"           entry,
F_RSTACK s" r@"           entry,
F_RSTACK s" 2>r"          entry,
F_RSTACK s" 2r>"          entry,
F_RSTACK s" 2r@"          entry,

\ ---- Control flow (special handling required) ----
F_CONTROL s" if"          entry,
F_CONTROL s" then"        entry,
F_CONTROL s" else"        entry,
F_CONTROL s" begin"       entry,
F_CONTROL s" while"       entry,
F_CONTROL s" repeat"      entry,
F_CONTROL s" until"       entry,
F_CONTROL s" again"       entry,
F_CONTROL s" do"          entry,
F_CONTROL s" ?do"         entry,
F_CONTROL s" loop"        entry,
F_CONTROL s" +loop"       entry,
F_CONTROL s" i"           entry,
F_CONTROL s" j"           entry,
F_CONTROL s" leave"       entry,
F_CONTROL s" unloop"      entry,
F_CONTROL s" exit"        entry,
F_CONTROL s" recurse"     entry,
F_CONTROL s" recursive"   entry,
F_CONTROL s" execute"     entry,
F_CONTROL s" ["           entry,
F_CONTROL s" ]"           entry,
F_CONTROL s" literal"     entry,
F_CONTROL s" postpone"    entry,
F_CONTROL s" does>"       entry,
F_CONTROL s" quit"        entry,
F_CONTROL s" abort"       entry,
F_CONTROL s" throw"       entry,
F_CONTROL s" [char]"      entry,

\ ---- Fused comparison+branch (special optimization) ----
F_CONTROL s" <if"         entry,
F_CONTROL s" >if"         entry,
F_CONTROL s" =if"         entry,
F_CONTROL s" 0<if"        entry,
F_CONTROL s" 0=if"        entry,
F_CONTROL s" 0=until"     entry,
F_CONTROL s" nzloop"      entry,
F_CONTROL s" 1-nzloop"    entry,

end-table

table-here @ builtin-table - constant BUILTIN-TABLE-SIZE

\ ============================================================
\ LOOKUP INTERFACE
\ ============================================================

: entry-name ( entry -- addr u )
  \ Get name from entry (find length by scanning for null)
  dup 16 0 do
    dup i + c@ 0= if drop i unloop exit then
  loop
  drop 16 ;

: entry-flags ( entry -- flags )
  \ Get flags from entry (at offset 16)
  16 + @ ;

: name= ( addr1 u1 addr2 u2 -- flag )
  \ Compare two strings for equality
  rot over <> if 2drop drop false exit then
  0 ?do
    over i + c@ over i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

: find-builtin ( addr u -- flags true | false )
  \ Look up word in builtin table
  \ Returns flags if found, false if not found
  builtin-table
  begin
    dup c@ 0 <> while             \ while name not empty
    >r                           \ save entry addr
    2dup r@ entry-name name=
    if
      2drop r> entry-flags true exit
    then
    r> ENTRY-SIZE +              \ next entry
  repeat
  drop 2drop false ;

\ ============================================================
\ FLAG TESTING HELPERS
\ ============================================================

: foldable1? ( flags -- flag )
  \ Can this operation be folded with 1 constant?
  F_FOLD1 and 0 <> ;

: foldable2? ( flags -- flag )
  \ Can this operation be folded with 2 constants?
  F_FOLD2 and 0 <> ;

: commutative? ( flags -- flag )
  \ Is this operation commutative (swap can be absorbed)?
  F_COMMUT and 0 <> ;

: has-io? ( flags -- flag )
  \ Does this operation have I/O side effects?
  F_IO and 0 <> ;

: stack-op? ( flags -- flag )
  \ Is this a pure stack manipulation?
  F_STACKOP and 0 <> ;

: control-flow? ( flags -- flag )
  \ Is this a control flow word?
  F_CONTROL and 0 <> ;

\ ============================================================
\ DEBUGGING / INTROSPECTION
\ ============================================================

: .flags ( flags -- )
  \ Print flag names for debugging
  dup F_FOLD1 and if ." fold1 " then
  dup F_FOLD2 and if ." fold2 " then
  dup F_COMMUT and if ." commut " then
  dup F_STACKOP and if ." stack " then
  dup F_IO and if ." io " then
  dup F_CONTROL and if ." ctrl " then
  dup F_MEMORY and if ." mem " then
  F_RSTACK and if ." rstack " then ;

: .builtin-entry ( entry -- )
  \ Print one table entry for debugging
  dup entry-name type
  16 over entry-name nip - spaces  \ align
  entry-flags .flags cr ;

: .builtins ( -- )
  \ Print all builtins in table
  builtin-table
  begin
    dup c@ 0 <> while
    dup .builtin-entry
    ENTRY-SIZE +
  repeat
  drop ;

: count-builtins ( -- n )
  \ Count entries in table
  0 builtin-table
  begin
    dup c@ 0 <> while
    swap 1+ swap
    ENTRY-SIZE +
  repeat
  drop ;

