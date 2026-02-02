\ tools/vocab.fs - Fifth Word Vocabulary Database
\ Complete catalog of interpreter primitives with metadata
\ Format: pipe-delimited entries compiled into a flat buffer

require lib/str.fs

\ ============================================================
\ CATEGORIES
\ ============================================================

0 constant CAT-STACK
1 constant CAT-ARITH
2 constant CAT-CMP
3 constant CAT-LOGIC
4 constant CAT-MEM
5 constant CAT-IO
6 constant CAT-CTRL
7 constant CAT-STR
8 constant CAT-META
9 constant CAT-NUM
10 constant CAT-MISC
11 constant CAT-RSTACK
12 constant NUM-CATS

\ Category name lookup
: cat-name ( n -- addr u )
  case
    0 of s" Stack"       endof
    1 of s" Arithmetic"  endof
    2 of s" Comparison"  endof
    3 of s" Logic/Bits"  endof
    4 of s" Memory"      endof
    5 of s" I/O"         endof
    6 of s" Control"     endof
    7 of s" String"      endof
    8 of s" Meta"        endof
    9 of s" NumericOut"  endof
    10 of s" Misc"       endof
    11 of s" ReturnStack" endof
    s" Unknown" rot
  endcase ;

\ ============================================================
\ VOCABULARY STORAGE
\ ============================================================
\ Each entry: 80 bytes compiled directly via here
\ Fields: name(16) cat(1) compiled(1) effect(24) desc(32) pad(6) = 80

80 constant VENTRY
200 constant MAX-WORDS
create vocab-buf MAX-WORDS VENTRY * allot
vocab-buf MAX-WORDS VENTRY * 0 fill
variable vocab-count  0 vocab-count !

0  constant VF-NAME    \ 16 bytes
16 constant VF-CAT     \ 1 byte
17 constant VF-COMP    \ 1 byte
18 constant VF-EFFECT  \ 24 bytes
42 constant VF-DESC    \ 32 bytes
74 constant VF-FREQ    \ 1 byte (uses padding)

: vocab-entry ( n -- addr ) VENTRY * vocab-buf + ;

\ Set one field (copies string immediately)
: vset-name ( addr u -- )
  15 min >r vocab-count @ vocab-entry VF-NAME + r> move ;

: vset-effect ( addr u -- )
  23 min >r vocab-count @ vocab-entry VF-EFFECT + r> move ;

: vset-desc ( addr u -- )
  31 min >r vocab-count @ vocab-entry VF-DESC + r> move ;

: vset-cat ( cat -- )
  vocab-count @ vocab-entry VF-CAT + c! ;

: vset-comp ( flag -- )
  vocab-count @ vocab-entry VF-COMP + c! ;

: vset-freq ( freq -- )
  vocab-count @ vocab-entry VF-FREQ + c! ;

: vnext ( -- )
  1 vocab-count +! ;

\ Convenience: define an entry with separate calls (avoids s" transient issue)
\ Usage:
\   s" dup" vset-name  CAT-STACK vset-cat  1 vset-comp
\   s" ( x -- x x )" vset-effect  s" Duplicate TOS" vset-desc  vnext

\ Get field from entry
variable _slen
: strlen-at ( addr max -- u )
  0 _slen !
  begin dup 0> while
    over c@ 0= if 2drop _slen @ exit then
    1 _slen +! 1- swap 1+ swap
  repeat 2drop _slen @ ;

: vname ( n -- addr u )
  vocab-entry VF-NAME + dup 16 strlen-at ;

: vcat ( n -- cat ) vocab-entry VF-CAT + c@ ;
: vcomp ( n -- flag ) vocab-entry VF-COMP + c@ ;
: vfreq ( n -- freq ) vocab-entry VF-FREQ + c@ ;

create freq-tmp 8 allot
: freq-name ( n -- addr u )
  <# [char] % hold #s #>   \ ( pno-addr u )
  7 min >r                  \ ( pno-addr ) R: u'
  freq-tmp r@ move          \ move( src=pno-addr dst=freq-tmp count=u' )
  freq-tmp r> ;              \ ( freq-tmp u' )

: veffect ( n -- addr u )
  vocab-entry VF-EFFECT + dup 24 strlen-at ;

: vdesc ( n -- addr u )
  vocab-entry VF-DESC + dup 32 strlen-at ;

\ ============================================================
\ REGISTER ALL WORDS
\ ============================================================
\ Each word: set fields individually, then vnext

\ --- Stack Operations ---
s" dup"   vset-name CAT-STACK vset-cat 1 vset-comp 95 vset-freq s" ( x -- x x )" vset-effect s" Duplicate TOS" vset-desc vnext
s" drop"  vset-name CAT-STACK vset-cat 1 vset-comp 90 vset-freq s" ( x -- )" vset-effect s" Remove TOS" vset-desc vnext
s" swap"  vset-name CAT-STACK vset-cat 1 vset-comp 90 vset-freq s" ( a b -- b a )" vset-effect s" Swap top two" vset-desc vnext
s" over"  vset-name CAT-STACK vset-cat 1 vset-comp 80 vset-freq s" ( a b -- a b a )" vset-effect s" Copy second to top" vset-desc vnext
s" rot"   vset-name CAT-STACK vset-cat 1 vset-comp 55 vset-freq s" ( a b c -- b c a )" vset-effect s" Rotate third to top" vset-desc vnext
s" -rot"  vset-name CAT-STACK vset-cat 0 vset-comp 20 vset-freq s" ( a b c -- c a b )" vset-effect s" Reverse rotate" vset-desc vnext
s" nip"   vset-name CAT-STACK vset-cat 1 vset-comp 40 vset-freq s" ( a b -- b )" vset-effect s" Remove second" vset-desc vnext
s" tuck"  vset-name CAT-STACK vset-cat 1 vset-comp 35 vset-freq s" ( a b -- b a b )" vset-effect s" Copy TOS below second" vset-desc vnext
s" ?dup"  vset-name CAT-STACK vset-cat 0 vset-comp 25 vset-freq s" ( x -- x x | 0 )" vset-effect s" Dup if nonzero" vset-desc vnext
s" 2dup"  vset-name CAT-STACK vset-cat 1 vset-comp 60 vset-freq s" ( a b -- a b a b )" vset-effect s" Dup top pair" vset-desc vnext
s" 2drop" vset-name CAT-STACK vset-cat 1 vset-comp 55 vset-freq s" ( a b -- )" vset-effect s" Drop top pair" vset-desc vnext
s" 2swap" vset-name CAT-STACK vset-cat 0 vset-comp 15 vset-freq s" ( a b c d--c d a b)" vset-effect s" Swap top two pairs" vset-desc vnext
s" 2over" vset-name CAT-STACK vset-cat 0 vset-comp 10 vset-freq s" ( a b c d--..a b )" vset-effect s" Copy second pair" vset-desc vnext
s" depth" vset-name CAT-STACK vset-cat 0 vset-comp 15 vset-freq s" ( -- n )" vset-effect s" Stack depth" vset-desc vnext
s" pick"  vset-name CAT-STACK vset-cat 0 vset-comp 10 vset-freq s" ( n -- x )" vset-effect s" Copy nth item" vset-desc vnext

\ --- Return Stack ---
s" >r"    vset-name CAT-RSTACK vset-cat 1 vset-comp 80 vset-freq s" ( x -- )(R: -- x )" vset-effect s" Move to return stack" vset-desc vnext
s" r>"    vset-name CAT-RSTACK vset-cat 1 vset-comp 80 vset-freq s" ( -- x )(R: x -- )" vset-effect s" Move from return stack" vset-desc vnext
s" r@"    vset-name CAT-RSTACK vset-cat 1 vset-comp 45 vset-freq s" ( -- x )(R: x--x )" vset-effect s" Copy from return stack" vset-desc vnext
s" 2>r"   vset-name CAT-RSTACK vset-cat 1 vset-comp 35 vset-freq s" ( a b -- )(R:--a b)" vset-effect s" Move pair to rstack" vset-desc vnext
s" 2r>"   vset-name CAT-RSTACK vset-cat 1 vset-comp 35 vset-freq s" ( --a b )(R:a b--)" vset-effect s" Move pair from rstack" vset-desc vnext
s" 2r@"   vset-name CAT-RSTACK vset-cat 1 vset-comp 15 vset-freq s" ( --a b)(R:ab--ab)" vset-effect s" Copy pair from rstack" vset-desc vnext

\ --- Arithmetic ---
s" +"       vset-name CAT-ARITH vset-cat 1 vset-comp 95 vset-freq s" ( a b -- a+b )" vset-effect s" Add" vset-desc vnext
s" -"       vset-name CAT-ARITH vset-cat 1 vset-comp 85 vset-freq s" ( a b -- a-b )" vset-effect s" Subtract" vset-desc vnext
s" *"       vset-name CAT-ARITH vset-cat 1 vset-comp 70 vset-freq s" ( a b -- a*b )" vset-effect s" Multiply" vset-desc vnext
s" /"       vset-name CAT-ARITH vset-cat 1 vset-comp 50 vset-freq s" ( a b -- a/b )" vset-effect s" Divide" vset-desc vnext
s" mod"     vset-name CAT-ARITH vset-cat 1 vset-comp 40 vset-freq s" ( a b -- a%b )" vset-effect s" Modulo" vset-desc vnext
s" /mod"    vset-name CAT-ARITH vset-cat 0 vset-comp 15 vset-freq s" ( a b -- r q )" vset-effect s" Divide with remainder" vset-desc vnext
s" negate"  vset-name CAT-ARITH vset-cat 1 vset-comp 35 vset-freq s" ( n -- -n )" vset-effect s" Negate" vset-desc vnext
s" abs"     vset-name CAT-ARITH vset-cat 1 vset-comp 20 vset-freq s" ( n -- |n| )" vset-effect s" Absolute value" vset-desc vnext
s" min"     vset-name CAT-ARITH vset-cat 1 vset-comp 30 vset-freq s" ( a b -- min )" vset-effect s" Minimum" vset-desc vnext
s" max"     vset-name CAT-ARITH vset-cat 1 vset-comp 30 vset-freq s" ( a b -- max )" vset-effect s" Maximum" vset-desc vnext
s" 1+"      vset-name CAT-ARITH vset-cat 1 vset-comp 85 vset-freq s" ( n -- n+1 )" vset-effect s" Increment" vset-desc vnext
s" 1-"      vset-name CAT-ARITH vset-cat 1 vset-comp 75 vset-freq s" ( n -- n-1 )" vset-effect s" Decrement" vset-desc vnext
s" */"      vset-name CAT-ARITH vset-cat 0 vset-comp 5 vset-freq s" ( a b c -- a*b/c )" vset-effect s" Scale" vset-desc vnext
s" */mod"   vset-name CAT-ARITH vset-cat 0 vset-comp 5 vset-freq s" ( a b c -- r q )" vset-effect s" Scale with remainder" vset-desc vnext
s" 2+"      vset-name CAT-ARITH vset-cat 0 vset-comp 20 vset-freq s" ( n -- n+2 )" vset-effect s" Add two" vset-desc vnext
s" 2-"      vset-name CAT-ARITH vset-cat 0 vset-comp 15 vset-freq s" ( n -- n-2 )" vset-effect s" Subtract two" vset-desc vnext
s" 2*"      vset-name CAT-ARITH vset-cat 0 vset-comp 25 vset-freq s" ( n -- n*2 )" vset-effect s" Double" vset-desc vnext
s" 2/"      vset-name CAT-ARITH vset-cat 0 vset-comp 20 vset-freq s" ( n -- n/2 )" vset-effect s" Halve" vset-desc vnext

\ --- Comparison ---
s" ="   vset-name CAT-CMP vset-cat 1 vset-comp 75 vset-freq s" ( a b -- flag )" vset-effect s" Equal" vset-desc vnext
s" <>"  vset-name CAT-CMP vset-cat 1 vset-comp 30 vset-freq s" ( a b -- flag )" vset-effect s" Not equal" vset-desc vnext
s" <"   vset-name CAT-CMP vset-cat 1 vset-comp 65 vset-freq s" ( a b -- flag )" vset-effect s" Less than" vset-desc vnext
s" >"   vset-name CAT-CMP vset-cat 1 vset-comp 55 vset-freq s" ( a b -- flag )" vset-effect s" Greater than" vset-desc vnext
s" u<"  vset-name CAT-CMP vset-cat 0 vset-comp 10 vset-freq s" ( a b -- flag )" vset-effect s" Unsigned less than" vset-desc vnext
s" 0="  vset-name CAT-CMP vset-cat 1 vset-comp 60 vset-freq s" ( n -- flag )" vset-effect s" Equal to zero" vset-desc vnext
s" 0<"  vset-name CAT-CMP vset-cat 1 vset-comp 25 vset-freq s" ( n -- flag )" vset-effect s" Less than zero" vset-desc vnext
s" 0>"  vset-name CAT-CMP vset-cat 1 vset-comp 20 vset-freq s" ( n -- flag )" vset-effect s" Greater than zero" vset-desc vnext
s" 0<>" vset-name CAT-CMP vset-cat 0 vset-comp 15 vset-freq s" ( n -- flag )" vset-effect s" Not zero" vset-desc vnext
s" <="  vset-name CAT-CMP vset-cat 0 vset-comp 25 vset-freq s" ( a b -- flag )" vset-effect s" Less or equal" vset-desc vnext
s" >="  vset-name CAT-CMP vset-cat 0 vset-comp 25 vset-freq s" ( a b -- flag )" vset-effect s" Greater or equal" vset-desc vnext
s" u>"  vset-name CAT-CMP vset-cat 0 vset-comp 5 vset-freq s" ( a b -- flag )" vset-effect s" Unsigned greater" vset-desc vnext
s" within" vset-name CAT-CMP vset-cat 0 vset-comp 15 vset-freq s" ( n lo hi -- flag )" vset-effect s" In range [lo,hi)" vset-desc vnext

\ --- Logic/Bitwise ---
s" and"    vset-name CAT-LOGIC vset-cat 1 vset-comp 50 vset-freq s" ( a b -- a&b )" vset-effect s" Bitwise AND" vset-desc vnext
s" or"     vset-name CAT-LOGIC vset-cat 1 vset-comp 45 vset-freq s" ( a b -- a|b )" vset-effect s" Bitwise OR" vset-desc vnext
s" xor"    vset-name CAT-LOGIC vset-cat 1 vset-comp 20 vset-freq s" ( a b -- a^b )" vset-effect s" Bitwise XOR" vset-desc vnext
s" invert" vset-name CAT-LOGIC vset-cat 1 vset-comp 20 vset-freq s" ( n -- ~n )" vset-effect s" Bitwise NOT" vset-desc vnext
s" lshift" vset-name CAT-LOGIC vset-cat 1 vset-comp 15 vset-freq s" ( n u -- n<<u )" vset-effect s" Left shift" vset-desc vnext
s" rshift" vset-name CAT-LOGIC vset-cat 1 vset-comp 15 vset-freq s" ( n u -- n>>u )" vset-effect s" Right shift" vset-desc vnext

\ --- Memory ---
s" @"       vset-name CAT-MEM vset-cat 1 vset-comp 90 vset-freq s" ( addr -- x )" vset-effect s" Fetch cell" vset-desc vnext
s" !"       vset-name CAT-MEM vset-cat 1 vset-comp 85 vset-freq s" ( x addr -- )" vset-effect s" Store cell" vset-desc vnext
s" c@"      vset-name CAT-MEM vset-cat 1 vset-comp 45 vset-freq s" ( addr -- c )" vset-effect s" Fetch byte" vset-desc vnext
s" c!"      vset-name CAT-MEM vset-cat 1 vset-comp 40 vset-freq s" ( c addr -- )" vset-effect s" Store byte" vset-desc vnext
s" +!"      vset-name CAT-MEM vset-cat 1 vset-comp 40 vset-freq s" ( n addr -- )" vset-effect s" Add to cell" vset-desc vnext
s" here"    vset-name CAT-MEM vset-cat 1 vset-comp 35 vset-freq s" ( -- addr )" vset-effect s" Dictionary pointer" vset-desc vnext
s" allot"   vset-name CAT-MEM vset-cat 0 vset-comp 20 vset-freq s" ( n -- )" vset-effect s" Allocate n bytes" vset-desc vnext
s" cells"   vset-name CAT-MEM vset-cat 1 vset-comp 40 vset-freq s" ( n -- n*cell )" vset-effect s" Cells to bytes" vset-desc vnext
s" cell+"   vset-name CAT-MEM vset-cat 1 vset-comp 20 vset-freq s" ( a -- a+cell )" vset-effect s" Advance one cell" vset-desc vnext
s" ,"       vset-name CAT-MEM vset-cat 0 vset-comp 10 vset-freq s" ( x -- )" vset-effect s" Compile cell" vset-desc vnext
s" c,"      vset-name CAT-MEM vset-cat 0 vset-comp 5 vset-freq s" ( c -- )" vset-effect s" Compile byte" vset-desc vnext
s" move"    vset-name CAT-MEM vset-cat 0 vset-comp 25 vset-freq s" ( src dst u -- )" vset-effect s" Copy memory" vset-desc vnext
s" fill"    vset-name CAT-MEM vset-cat 0 vset-comp 15 vset-freq s" ( addr u c -- )" vset-effect s" Fill memory" vset-desc vnext
s" /string" vset-name CAT-MEM vset-cat 0 vset-comp 15 vset-freq s" ( a u n -- a+n u-n)" vset-effect s" Advance string" vset-desc vnext
s" count"   vset-name CAT-MEM vset-cat 0 vset-comp 10 vset-freq s" ( a -- a+1 u )" vset-effect s" Counted string" vset-desc vnext
s" variable" vset-name CAT-MEM vset-cat 0 vset-comp 30 vset-freq s" ( -- )" vset-effect s" Create variable" vset-desc vnext
s" constant" vset-name CAT-MEM vset-cat 0 vset-comp 35 vset-freq s" ( n -- )" vset-effect s" Create constant" vset-desc vnext

\ --- Compiler/Meta ---
s" :"         vset-name CAT-META vset-cat 0 vset-comp 95 vset-freq s" ( -- )" vset-effect s" Begin definition" vset-desc vnext
s" ;"         vset-name CAT-META vset-cat 0 vset-comp 95 vset-freq s" ( -- )" vset-effect s" End definition" vset-desc vnext
s" immediate" vset-name CAT-META vset-cat 0 vset-comp 15 vset-freq s" ( -- )" vset-effect s" Mark word immediate" vset-desc vnext
s" ["         vset-name CAT-META vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Enter interpret mode" vset-desc vnext
s" ]"         vset-name CAT-META vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Enter compile mode" vset-desc vnext
s" state"     vset-name CAT-META vset-cat 0 vset-comp 5 vset-freq s" ( -- addr )" vset-effect s" Compilation state" vset-desc vnext
s" '"         vset-name CAT-META vset-cat 0 vset-comp 20 vset-freq s" ( -- xt )" vset-effect s" Find execution token" vset-desc vnext
s" [']"       vset-name CAT-META vset-cat 0 vset-comp 15 vset-freq s" ( -- xt )" vset-effect s" Compile-time tick" vset-desc vnext
s" execute"   vset-name CAT-META vset-cat 0 vset-comp 20 vset-freq s" ( xt -- )" vset-effect s" Execute token" vset-desc vnext
s" >body"     vset-name CAT-META vset-cat 0 vset-comp 5 vset-freq s" ( xt -- addr )" vset-effect s" Token to body" vset-desc vnext
s" create"    vset-name CAT-META vset-cat 0 vset-comp 30 vset-freq s" ( -- )" vset-effect s" Create dict entry" vset-desc vnext
s" find"      vset-name CAT-META vset-cat 0 vset-comp 5 vset-freq s" ( a u -- xt flag )" vset-effect s" Dictionary lookup" vset-desc vnext
s" literal"   vset-name CAT-META vset-cat 0 vset-comp 10 vset-freq s" ( n -- )" vset-effect s" Compile number" vset-desc vnext
s" compile,"  vset-name CAT-META vset-cat 0 vset-comp 5 vset-freq s" ( xt -- )" vset-effect s" Compile token" vset-desc vnext
s" postpone"  vset-name CAT-META vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Postpone word" vset-desc vnext
s" does>"     vset-name CAT-META vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Define runtime" vset-desc vnext
s" recurse"   vset-name CAT-META vset-cat 1 vset-comp 30 vset-freq s" ( -- )" vset-effect s" Call self" vset-desc vnext

\ --- Control Flow ---
s" if"      vset-name CAT-CTRL vset-cat 1 vset-comp 90 vset-freq s" ( flag -- )" vset-effect s" Conditional branch" vset-desc vnext
s" else"    vset-name CAT-CTRL vset-cat 1 vset-comp 70 vset-freq s" ( -- )" vset-effect s" Alternative branch" vset-desc vnext
s" then"    vset-name CAT-CTRL vset-cat 1 vset-comp 90 vset-freq s" ( -- )" vset-effect s" End conditional" vset-desc vnext
s" begin"   vset-name CAT-CTRL vset-cat 1 vset-comp 75 vset-freq s" ( -- )" vset-effect s" Start loop" vset-desc vnext
s" while"   vset-name CAT-CTRL vset-cat 1 vset-comp 65 vset-freq s" ( flag -- )" vset-effect s" Loop condition" vset-desc vnext
s" repeat"  vset-name CAT-CTRL vset-cat 1 vset-comp 65 vset-freq s" ( -- )" vset-effect s" End begin..while" vset-desc vnext
s" until"   vset-name CAT-CTRL vset-cat 1 vset-comp 35 vset-freq s" ( flag -- )" vset-effect s" Loop until true" vset-desc vnext
s" again"   vset-name CAT-CTRL vset-cat 1 vset-comp 15 vset-freq s" ( -- )" vset-effect s" Infinite loop end" vset-desc vnext
s" do"      vset-name CAT-CTRL vset-cat 1 vset-comp 70 vset-freq s" ( lim idx -- )" vset-effect s" Start counted loop" vset-desc vnext
s" ?do"     vset-name CAT-CTRL vset-cat 0 vset-comp 20 vset-freq s" ( lim idx -- )" vset-effect s" Conditional do" vset-desc vnext
s" loop"    vset-name CAT-CTRL vset-cat 1 vset-comp 70 vset-freq s" ( -- )" vset-effect s" End counted loop" vset-desc vnext
s" +loop"   vset-name CAT-CTRL vset-cat 1 vset-comp 25 vset-freq s" ( n -- )" vset-effect s" Loop with step" vset-desc vnext
s" i"       vset-name CAT-CTRL vset-cat 1 vset-comp 70 vset-freq s" ( -- n )" vset-effect s" Loop index" vset-desc vnext
s" j"       vset-name CAT-CTRL vset-cat 1 vset-comp 20 vset-freq s" ( -- n )" vset-effect s" Outer loop index" vset-desc vnext
s" unloop"  vset-name CAT-CTRL vset-cat 0 vset-comp 15 vset-freq s" ( -- )" vset-effect s" Remove loop params" vset-desc vnext
s" case"    vset-name CAT-CTRL vset-cat 0 vset-comp 25 vset-freq s" ( -- )" vset-effect s" Start case" vset-desc vnext
s" of"      vset-name CAT-CTRL vset-cat 0 vset-comp 25 vset-freq s" ( x1 x2 -- )" vset-effect s" Test case match" vset-desc vnext
s" endof"   vset-name CAT-CTRL vset-cat 0 vset-comp 25 vset-freq s" ( -- )" vset-effect s" End case branch" vset-desc vnext
s" endcase" vset-name CAT-CTRL vset-cat 0 vset-comp 25 vset-freq s" ( x -- )" vset-effect s" End case" vset-desc vnext
s" exit"    vset-name CAT-CTRL vset-cat 1 vset-comp 40 vset-freq s" ( -- )" vset-effect s" Return from word" vset-desc vnext
s" leave"   vset-name CAT-CTRL vset-cat 0 vset-comp 20 vset-freq s" ( -- )" vset-effect s" Exit do loop" vset-desc vnext
s" recursive" vset-name CAT-CTRL vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Allow recursion" vset-desc vnext

\ --- Strings ---
s\" s\""      vset-name CAT-STR vset-cat 1 vset-comp 85 vset-freq s" ( -- addr u )" vset-effect s" String literal" vset-desc vnext
s\" .\""      vset-name CAT-STR vset-cat 1 vset-comp 60 vset-freq s" ( -- )" vset-effect s" Print string literal" vset-desc vnext
s" .("        vset-name CAT-STR vset-cat 0 vset-comp 5 vset-freq s" ( -- )" vset-effect s" Print immediate" vset-desc vnext
s" [char]"    vset-name CAT-STR vset-cat 0 vset-comp 30 vset-freq s" ( -- c )" vset-effect s" Compile char literal" vset-desc vnext
s" char"      vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( -- c )" vset-effect s" Parse char" vset-desc vnext
s" parse-name" vset-name CAT-STR vset-cat 0 vset-comp 5 vset-freq s" ( -- addr u )" vset-effect s" Parse next word" vset-desc vnext
s\" abort\""  vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( flag -- )" vset-effect s" Conditional abort" vset-desc vnext
s" source"  vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( -- addr u )" vset-effect s" Input buffer" vset-desc vnext
s" >in"     vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( -- addr )" vset-effect s" Input offset var" vset-desc vnext
s" parse"   vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( c -- addr u )" vset-effect s" Parse to delimiter" vset-desc vnext
s" word"    vset-name CAT-STR vset-cat 0 vset-comp 5 vset-freq s" ( c -- addr )" vset-effect s" Parse word" vset-desc vnext
s" char+"   vset-name CAT-STR vset-cat 0 vset-comp 10 vset-freq s" ( a -- a+1 )" vset-effect s" Next char address" vset-desc vnext
s" chars"   vset-name CAT-STR vset-cat 0 vset-comp 5 vset-freq s" ( n -- n )" vset-effect s" Chars to bytes" vset-desc vnext

\ --- I/O ---
s" ."      vset-name CAT-IO vset-cat 1 vset-comp 80 vset-freq s" ( n -- )" vset-effect s" Print number" vset-desc vnext
s" u."     vset-name CAT-IO vset-cat 0 vset-comp 10 vset-freq s" ( u -- )" vset-effect s" Print unsigned" vset-desc vnext
s" .s"     vset-name CAT-IO vset-cat 0 vset-comp 25 vset-freq s" ( -- )" vset-effect s" Print stack" vset-desc vnext
s" emit"   vset-name CAT-IO vset-cat 1 vset-comp 50 vset-freq s" ( c -- )" vset-effect s" Emit character" vset-desc vnext
s" type"   vset-name CAT-IO vset-cat 1 vset-comp 75 vset-freq s" ( addr u -- )" vset-effect s" Print string" vset-desc vnext
s" cr"     vset-name CAT-IO vset-cat 1 vset-comp 80 vset-freq s" ( -- )" vset-effect s" Newline" vset-desc vnext
s" key"    vset-name CAT-IO vset-cat 0 vset-comp 20 vset-freq s" ( -- c )" vset-effect s" Read character" vset-desc vnext
s" accept" vset-name CAT-IO vset-cat 0 vset-comp 10 vset-freq s" ( addr u -- u' )" vset-effect s" Read line" vset-desc vnext
s" space"  vset-name CAT-IO vset-cat 0 vset-comp 30 vset-freq s" ( -- )" vset-effect s" Emit space" vset-desc vnext
s" spaces" vset-name CAT-IO vset-cat 0 vset-comp 15 vset-freq s" ( n -- )" vset-effect s" Emit n spaces" vset-desc vnext

\ --- Numeric Output ---
s" <#"   vset-name CAT-NUM vset-cat 0 vset-comp 15 vset-freq s" ( -- )" vset-effect s" Begin PNO" vset-desc vnext
s" #"    vset-name CAT-NUM vset-cat 0 vset-comp 10 vset-freq s" ( n -- n' )" vset-effect s" Add one digit" vset-desc vnext
s" #s"   vset-name CAT-NUM vset-cat 0 vset-comp 15 vset-freq s" ( n -- 0 )" vset-effect s" Add all digits" vset-desc vnext
s" #>"   vset-name CAT-NUM vset-cat 0 vset-comp 15 vset-freq s" ( n -- addr u )" vset-effect s" End PNO" vset-desc vnext
s" hold" vset-name CAT-NUM vset-cat 0 vset-comp 5 vset-freq s" ( c -- )" vset-effect s" Insert char in PNO" vset-desc vnext
s" sign" vset-name CAT-NUM vset-cat 0 vset-comp 5 vset-freq s" ( n -- )" vset-effect s" Conditional minus" vset-desc vnext
s" base"    vset-name CAT-NUM vset-cat 0 vset-comp 15 vset-freq s" ( -- addr )" vset-effect s" Number base var" vset-desc vnext
s" decimal" vset-name CAT-NUM vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Set base 10" vset-desc vnext

\ --- Double Precision ---
s" s>d"    vset-name CAT-ARITH vset-cat 0 vset-comp 15 vset-freq s" ( n -- d )" vset-effect s" Single to double" vset-desc vnext
s" d+"     vset-name CAT-ARITH vset-cat 0 vset-comp 10 vset-freq s" ( d1 d2 -- d )" vset-effect s" Double add" vset-desc vnext
s" d-"     vset-name CAT-ARITH vset-cat 0 vset-comp 10 vset-freq s" ( d1 d2 -- d )" vset-effect s" Double subtract" vset-desc vnext
s" um*"    vset-name CAT-ARITH vset-cat 0 vset-comp 10 vset-freq s" ( u1 u2 -- ud )" vset-effect s" Unsigned multiply" vset-desc vnext
s" m*"     vset-name CAT-ARITH vset-cat 0 vset-comp 10 vset-freq s" ( n1 n2 -- d )" vset-effect s" Signed multiply" vset-desc vnext
s" um/mod" vset-name CAT-ARITH vset-cat 0 vset-comp 10 vset-freq s" ( ud u -- r q )" vset-effect s" Unsigned div/mod" vset-desc vnext
s" sm/rem" vset-name CAT-ARITH vset-cat 0 vset-comp 5 vset-freq s" ( d n -- r q )" vset-effect s" Symmetric div/mod" vset-desc vnext
s" fm/mod" vset-name CAT-ARITH vset-cat 0 vset-comp 5 vset-freq s" ( d n -- r q )" vset-effect s" Floored div/mod" vset-desc vnext

\ --- Misc ---
s" noop"  vset-name CAT-MISC vset-cat 0 vset-comp 5 vset-freq s" ( -- )" vset-effect s" No operation" vset-desc vnext
s" true"  vset-name CAT-MISC vset-cat 0 vset-comp 30 vset-freq s" ( -- -1 )" vset-effect s" True flag" vset-desc vnext
s" false" vset-name CAT-MISC vset-cat 0 vset-comp 25 vset-freq s" ( -- 0 )" vset-effect s" False flag" vset-desc vnext
s" bl"    vset-name CAT-MISC vset-cat 0 vset-comp 10 vset-freq s" ( -- 32 )" vset-effect s" Space character" vset-desc vnext
s" abort" vset-name CAT-MISC vset-cat 0 vset-comp 10 vset-freq s" ( -- )" vset-effect s" Abort execution" vset-desc vnext
s" argc"  vset-name CAT-MISC vset-cat 0 vset-comp 5 vset-freq s" ( -- n )" vset-effect s" Argument count" vset-desc vnext
s" argv"  vset-name CAT-MISC vset-cat 0 vset-comp 5 vset-freq s" ( n -- addr u )" vset-effect s" Argument string" vset-desc vnext
s" s>number?" vset-name CAT-MISC vset-cat 0 vset-comp 10 vset-freq s" ( a u -- n 0 f )" vset-effect s" Parse number" vset-desc vnext
s" >number"   vset-name CAT-MISC vset-cat 0 vset-comp 5 vset-freq s" ( ud a u--ud' a' u')" vset-effect s" Convert number" vset-desc vnext

\ ============================================================
\ STATISTICS
\ ============================================================

: vocab-total ( -- n ) vocab-count @ ;
: vocab-compiled ( -- n )
  0 vocab-count @ 0 ?do i vcomp + loop ;

: vocab-cat-total ( cat -- n )
  0 vocab-count @ 0 ?do over i vcat = if 1+ then loop nip ;

: vocab-cat-compiled ( cat -- n )
  0 vocab-count @ 0 ?do
    over i vcat = i vcomp and if 1+ then
  loop nip ;

\ ============================================================
\ AUTOMATIC COMPILED DETECTION
\ ============================================================
\ Scans compiler/sixth.fs to detect which words are actually
\ compiled. Runs at load time. Overwrites hardcoded values.

create scan-buf 8192 allot
variable scan-len

\ Reset all compiled flags to 0
: reset-compiled ( -- )
  vocab-count @ 0 ?do
    0 i vocab-entry VF-COMP + c!
  loop ;

\ Find word in vocab by name, return index or -1
: find-vocab-word ( addr u -- n | -1 )
  vocab-count @ 0 ?do
    2dup i vname str= if
      2drop i unloop exit
    then
  loop
  2drop -1 ;

\ Mark a word as compiled by name
: mark-compiled ( addr u -- )
  find-vocab-word dup -1 <> if
    vocab-entry VF-COMP + 1 swap c!
  else drop then ;

\ Parse newline-separated word list
variable _line-start
variable _line-len

: parse-word-list ( addr u -- )
  over _line-start !
  0 _line-len !
  begin dup 0> while
    over c@ 10 = if
      _line-start @ _line-len @
      dup 0> if mark-compiled else 2drop then
      over 1+ _line-start !
      0 _line-len !
    else
      1 _line-len +!
    then
    1- swap 1+ swap
  repeat
  _line-len @ 0> if
    _line-start @ _line-len @ mark-compiled
  then
  2drop ;

\ Scan sixth.fs for compiled words
: scan-sixth ( -- )
  reset-compiled
  s" grep 'str= if' compiler/sixth.fs 2>/dev/null | grep -o '\$[^ ]* str=' | sed 's/^\$//;s/ str=$//' | sort -u > /tmp/fifth-scan.txt" system
  s" /tmp/fifth-scan.txt" slurp-file   ( addr u )
  dup 0= if 2drop exit then
  dup 8191 > if drop 8191 then         ( addr u' )
  dup scan-len !                       ( addr u' )
  scan-buf swap move                   ( )
  scan-buf scan-len @ parse-word-list ;

\ Run at load time
scan-sixth
