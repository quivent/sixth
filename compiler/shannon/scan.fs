\ compiler/shannon/scan.fs - Two-Pass Scanner (Pass 1)
\ Part of Shannon Architecture refactoring
\
\ This module implements Pass 1 of the two-pass compiler. It scans
\ the entire source file and gathers metadata about every word
\ definition before code generation begins.
\
\ Why two-pass?
\ - Forward references resolved with correct argument counts
\ - Call-count tracking enables inline vs call decisions
\ - Dead code elimination knows what's actually called
\ - Full program knowledge before emitting any code

\ ============================================================
\ INFO-BUF: Word Metadata Table
\ ============================================================
\ Each word definition gets a 40-byte entry:
\
\   Offset  Size  Field       Description
\   ------  ----  ----------  --------------------------------
\   0-23    24    name        Word name (null-padded)
\   24      1     nargs       Input count from ( ... -- ... )
\   25      1     rets        Output count + 1 (0 = no comment)
\   26-27   2     flags       bit0=recursive, bit1=has-io
\   28-29   2     call-count  Times this word is referenced
\   30-33   4     body-pos    Source position where body starts
\   34-37   4     code-addr   Code offset (0 = not yet compiled)
\   38-39   2     (padding)
\
\ The rets field stores rets+1 so that 0 means "no stack comment
\ found" and we can distinguish void (1) from 1-return (2).

512 constant INFO-MAX
create info-buf INFO-MAX 40 * allot
variable info-count  0 info-count !

\ ============================================================
\ INFO-BUF ACCESS
\ ============================================================

: info-entry ( i -- addr )
  \ Return address of entry i in info-buf
  40 * info-buf + ;

: info-name= ( addr u entry -- flag )
  \ Compare string (addr u) against name in entry
  >r
  dup 23 > if 2drop r> drop false exit then    \ too long
  r@ over + c@ 0 <> if r> drop 2drop false exit then  \ length mismatch
  r> swap 0 ?do
    2dup i + c@ swap i + c@ <> if 2drop false unloop exit then
  loop 2drop true ;

: info-find ( addr u -- entry | 0 )
  \ Find word by name, return entry address or 0
  info-count @ 0 ?do
    2dup i info-entry info-name= if
      2drop i info-entry unloop exit
    then
  loop
  2drop 0 ;

\ ============================================================
\ SCANNING STATE
\ ============================================================
\ These variables accumulate info about the current definition.

variable scan-nargs      \ input count from stack comment
variable scan-rets       \ output count from stack comment
variable scan-void       \ flag: no outputs?
variable scan-io         \ flag: contains I/O words?

create scan-name-buf 24 allot
variable scan-name-len
variable scan-body-pos   \ source position where body starts

\ ============================================================
\ STRING CONSTANTS FOR SCANNING
\ ============================================================
\ These identify tokens during scanning. We use create/c, to
\ store persistent strings (s" alone is transient).

create $:-str     1 c, char : c,
: $: $:-str 1+ 1 ;

create $;-str     1 c, char ; c,
: $; $;-str 1+ 1 ;

create $.-str     1 c, char . c,
: $. $.-str 1+ 1 ;

create $cr-str    2 c, char c c, char r c,
: $cr $cr-str 1+ 2 ;

create $emit-str  4 c, char e c, char m c, char i c, char t c,
: $emit $emit-str 1+ 4 ;

create $type-str  4 c, char t c, char y c, char p c, char e c,
: $type $type-str 1+ 4 ;

create $space-str 5 c, char s c, char p c, char a c, char c c, char e c,
: $space $space-str 1+ 5 ;

create $spaces-str 6 c, char s c, char p c, char a c, char c c, char e c, char s c,
: $spaces $spaces-str 1+ 6 ;

create $u.-str    2 c, char u c, char . c,
: $u. $u.-str 1+ 2 ;

create $key-str   3 c, char k c, char e c, char y c,
: $key $key-str 1+ 3 ;

\ ============================================================
\ WHITESPACE HANDLING
\ ============================================================

: scan-skip-ws ( -- )
  \ Skip whitespace during stack comment parsing
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ dup 32 <= swap 0 > and if
      1 input-pos +!
    else exit then
  again ;

\ ============================================================
\ STACK COMMENT PARSER
\ ============================================================
\ Parses ( arg1 arg2 -- ret1 ret2 ) to extract argument counts.
\ Sets scan-nargs, scan-rets, scan-void.

: scan-stack-comment ( -- )
  scan-skip-ws
  input-pos @ input-len @ >= if exit then
  \ Must start with ( followed by whitespace
  input-buf input-pos @ + c@ [char] ( <> if exit then
  input-pos @ 1+ input-len @ >= if exit then
  input-buf input-pos @ 1+ + c@ 32 > if exit then
  1 input-pos +!
  \ Initialize: void until we see output, 0 args/rets
  1 scan-void !  0 scan-nargs !  0 scan-rets !
  0 >r  \ r: 0=before --, 1=after --
  begin
    scan-skip-ws
    input-pos @ input-len @ >= if r> drop exit then
    input-buf input-pos @ + c@
    dup [char] ) = if drop r> drop 1 input-pos +! exit then
    dup [char] - = if
      \ Check for --
      input-pos @ 1+ input-len @ < if
        input-buf input-pos @ 1+ + c@ [char] - = if
          drop 2 input-pos +! r> drop 1 >r
        else
          drop
          begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
          r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
        then
      else
        drop
        begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
        r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
      then
    else
      drop
      \ Skip token
      begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
      \ Count: before -- it's an arg, after -- it's a ret
      r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
    then
  again ;

\ ============================================================
\ BODY SCANNER
\ ============================================================
\ Scans the body of a definition until ; to detect I/O words
\ and count references to other words.

: scan-body-io ( -- )
  0 scan-io !
  begin
    get-token dup 0= if 2drop exit then
    2dup $; str= if 2drop exit then
    \ Check for I/O words
    2dup $. str= if 1 scan-io ! then
    2dup $cr str= if 1 scan-io ! then
    2dup $emit str= if 1 scan-io ! then
    2dup $space str= if 1 scan-io ! then
    2dup $spaces str= if 1 scan-io ! then
    2dup $u. str= if 1 scan-io ! then
    2dup $key str= if 1 scan-io ! then
    2dup $type str= if 1 scan-io ! then
    \ Increment call-count for referenced words (if already in info-buf)
    \ Note: only counts backward references (word must exist already)
    2dup info-find ?dup if
      28 + dup c@ 1+ 255 min swap c!   \ increment call-count (max 255)
    then
    2drop
  again ;

\ ============================================================
\ ADD INFO ENTRY
\ ============================================================

: scan-add-info ( -- )
  \ Add current word to info-buf
  info-count @ INFO-MAX >= if exit then
  info-count @ info-entry >r
  r@ 40 0 fill                                   \ clear entire entry
  scan-name-buf r@ scan-name-len @ dup 23 > if drop 23 then move
  scan-nargs @ r@ 24 + c!                        \ nargs at 24
  scan-rets @ 1+ r@ 25 + c!                      \ rets+1 at 25 (0=no comment)
  scan-io @ r@ 26 + c!                           \ flags at 26
  \ call-count at 28 - already 0 from fill
  scan-body-pos @ r@ 30 + !                      \ body-pos at 30
  \ code-addr at 34 - already 0, set when compiled
  r> drop
  1 info-count +! ;

\ ============================================================
\ MAIN ENTRY POINT
\ ============================================================

: scan-all ( -- )
  \ Pass 1: Scan entire source, build info-buf
  \ Requires: input-buf, input-pos, input-len set up
  \ Requires: get-token, str= available
  0 info-count !
  begin
    get-token dup 0= if 2drop exit then
    2dup $: str= if
      2drop
      get-token dup 0= if 2drop exit then
      \ Save name
      dup scan-name-len !
      dup 23 > if drop 23 then
      scan-name-buf over 0 fill
      scan-name-buf swap move
      \ Initialize scan state
      1 scan-void !  1 scan-nargs !  1 scan-rets !  0 scan-io !
      \ Parse stack comment
      scan-stack-comment
      \ Record body position for future inlining
      input-pos @ scan-body-pos !
      \ Scan body for I/O and call references
      scan-body-io
      \ Store in info-buf
      scan-add-info
    else
      2drop
    then
  again ;
