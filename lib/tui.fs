\ fifth/lib/tui.fs - Terminal UI Library
\ Raw mode, ANSI escape sequences, tab system, scrolling, input loop
\ No dynamic allocation. Uses emit for all terminal output.

require lib/str.fs

\ ============================================================
\ ANSI ESCAPE HELPERS
\ ============================================================

: esc ( -- ) 27 emit ;
: csi ( -- ) esc [char] [ emit ;
: tui-cr ( -- ) 13 emit 10 emit ;

\ Type buffer with CR+LF for raw mode
: tui-type ( addr u -- )
  0 ?do
    dup i + c@ 10 = if tui-cr else dup i + c@ emit then
  loop drop ;

\ Emit decimal number without trailing space (for ANSI sequences)
: emit-num ( n -- ) <# #s #> type ;

\ Screen control
: term-clear ( -- ) csi ." 2J" csi ." H" ;
: term-home  ( -- ) csi ." H" ;
: term-goto  ( row col -- ) csi swap emit-num [char] ; emit emit-num [char] H emit ;
: term-erase-line ( -- ) csi ." 2K" ;
: term-erase-down ( -- ) csi ." J" ;

\ Cursor
: cursor-hide ( -- ) csi ." ?25l" ;
: cursor-show ( -- ) csi ." ?25h" ;

\ Foreground colors
: fg-black   ( -- ) csi ." 30m" ;
: fg-red     ( -- ) csi ." 31m" ;
: fg-green   ( -- ) csi ." 32m" ;
: fg-yellow  ( -- ) csi ." 33m" ;
: fg-blue    ( -- ) csi ." 34m" ;
: fg-magenta ( -- ) csi ." 35m" ;
: fg-cyan    ( -- ) csi ." 36m" ;
: fg-white   ( -- ) csi ." 37m" ;

\ Background colors
: bg-black   ( -- ) csi ." 40m" ;
: bg-red     ( -- ) csi ." 41m" ;
: bg-green   ( -- ) csi ." 42m" ;
: bg-blue    ( -- ) csi ." 44m" ;
: bg-white   ( -- ) csi ." 47m" ;
: bg-gray    ( -- ) csi ." 100m" ;

\ Attributes
: attr-reset ( -- ) csi ." 0m" ;
: attr-bold  ( -- ) csi ." 1m" ;
: attr-dim   ( -- ) csi ." 2m" ;
: attr-rev   ( -- ) csi ." 7m" ;

\ ============================================================
\ TERMINAL SIZE
\ ============================================================

variable term-cols  80 term-cols !
variable term-rows  24 term-rows !

: strip-ws ( addr u -- addr u' )
  begin dup 0> while
    2dup + 1- c@
    dup 32 = over 10 = or over 13 = or over 9 = or
    nip while
    1-
  repeat then ;

create tui-tmp-buf 128 allot

: slurp-to-tmp ( addr u -- addr' u' )
  slurp-file dup 0> if
    dup 127 > if 2drop tui-tmp-buf 0 exit then
    2dup tui-tmp-buf swap move
    nip tui-tmp-buf swap
  then ;

: detect-size ( -- )
  s" tput cols > /tmp/fifth-tui-cols 2>/dev/null" system
  s" tput lines > /tmp/fifth-tui-lines 2>/dev/null" system
  s" /tmp/fifth-tui-cols" slurp-to-tmp
  dup 0> if strip-ws s>number? if drop term-cols ! else 2drop then else 2drop then
  s" /tmp/fifth-tui-lines" slurp-to-tmp
  dup 0> if strip-ws s>number? if drop term-rows ! else 2drop then else 2drop then ;

\ ============================================================
\ RAW MODE
\ ============================================================

create stty-save-buf 128 allot
variable stty-save-len  0 stty-save-len !

: tui-raw ( -- )
  s" stty -g > /tmp/fifth-tui-stty" system
  s" /tmp/fifth-tui-stty" slurp-to-tmp
  strip-ws dup stty-save-len ! stty-save-buf swap move
  s" stty raw -echo" system ;

: tui-cooked ( -- )
  stty-save-len @ 0> if
    str-reset s" stty " str+ stty-save-buf stty-save-len @ str+ str$ system
  else
    s" stty sane" system
  then ;

: tui-cleanup ( -- )
  cursor-show attr-reset tui-cooked ;

\ ============================================================
\ TAB SYSTEM
\ ============================================================

6 constant MAX-TABS
variable current-tab  0 current-tab !

\ Tab names: 6 entries, 20 chars each (1 byte length + 19 chars)
create tab-names MAX-TABS 20 * allot
tab-names MAX-TABS 20 * 0 fill

: tab-name! ( addr u n -- )
  20 * tab-names +            \ ( addr u slot )
  rot rot                     \ ( slot addr u )
  dup 19 min                  \ ( slot addr u u' )
  >r                          \ ( slot addr u ) R: u'
  drop                        \ ( slot addr )
  over 1+ r@ move             \ copy u' bytes from addr to slot+1
  r> swap c! ;                \ store length at slot[0]

: tab-name@ ( n -- addr u )
  20 * tab-names +
  dup c@ swap 1+ swap ;

\ Draw tab bar at row 1
: draw-tab-bar ( -- )
  1 1 term-goto
  term-erase-line
  MAX-TABS 0 do
    i current-tab @ = if
      attr-bold attr-rev
    else
      attr-dim
    then
    space i tab-name@ type space
    attr-reset
    i MAX-TABS 1- < if fg-cyan ." |" attr-reset then
  loop ;

\ ============================================================
\ SCROLLING
\ ============================================================

create scroll-offsets MAX-TABS cells allot
scroll-offsets MAX-TABS cells 0 fill

: scroll@ ( -- n ) current-tab @ cells scroll-offsets + @ ;
: scroll! ( n -- ) current-tab @ cells scroll-offsets + ! ;
: scroll-up ( -- ) scroll@ 1- 0 max scroll! ;
: scroll-down ( max -- ) scroll@ 1+ min scroll! ;
: content-rows ( -- n ) term-rows @ 4 - ;
: scroll-pgup ( -- ) scroll@ content-rows - 0 max scroll! ;
: scroll-pgdn ( max -- ) scroll@ content-rows + min scroll! ;

\ ============================================================
\ DRAWING HELPERS
\ ============================================================

\ Pad with spaces to fill width
: pad-to ( col -- )
  term-cols @ swap - 0 max 0 ?do space loop ;

\ Draw horizontal line
: hline ( row width -- )
  swap 1 term-goto
  0 ?do [char] - emit loop ;

\ Fill a line with spaces (clear it)
: clear-line ( row -- )
  1 term-goto term-erase-line ;

\ Status bar at bottom
variable status-msg-addr  0 status-msg-addr !
variable status-msg-len   0 status-msg-len !

: set-status ( addr u -- )
  dup status-msg-len ! status-msg-addr ! ;

: draw-status-bar ( -- )
  term-rows @ 1 term-goto
  term-erase-line
  attr-rev
  status-msg-len @ 0> if
    space status-msg-addr @ status-msg-len @ type
  else
    ."  q:Quit  1-6:Tabs  Arrows:Scroll  r:Refresh"
  then
  term-cols @ pad-to
  attr-reset ;

\ ============================================================
\ PROGRESS BAR
\ ============================================================

variable _pb-cur
variable _pb-tot
variable _pb-wid
: progress-bar ( current total width -- )
  _pb-wid ! swap _pb-cur ! dup _pb-tot !
  dup 0= if drop 0 else _pb-cur @ _pb-wid @ * swap / then
  [char] [ emit
  dup 0 ?do [char] = emit loop
  _pb-wid @ swap - 0 max 0 ?do [char] . emit loop
  [char] ] emit space
  _pb-cur @ emit-num [char] / emit _pb-tot @ emit-num ;

\ ============================================================
\ INPUT HANDLING
\ ============================================================

variable quit-flag  0 quit-flag !
variable refresh-flag  0 refresh-flag !

\ Deferred words for per-tab key handling and drawing
\ The main app sets these via vectors
variable 'draw-content   0 'draw-content !
variable 'handle-tab-key 0 'handle-tab-key !
variable 'on-refresh     0 'on-refresh !
variable 'handle-up-down 0 'handle-up-down !

\ Execute deferred word if set
: ?exec ( xt -- ) dup 0= if drop else execute then ;

: handle-arrow ( -- )
  key case
    [char] A of 'handle-up-down @ ?dup if -1 swap execute else scroll-up then endof
    [char] B of 'handle-up-down @ ?dup if  1 swap execute else 999 scroll-down then endof
    [char] C of current-tab @ 1+ MAX-TABS 1- min current-tab ! endof
    [char] D of current-tab @ 1- 0 max current-tab ! endof
  endcase ;

: handle-esc ( -- )
  key [char] [ = if handle-arrow then ;

: handle-key ( c -- )
  dup [char] q = if drop -1 quit-flag ! exit then
  dup [char] 1 = if drop 0 current-tab ! exit then
  dup [char] 2 = if drop 1 current-tab ! exit then
  dup [char] 3 = if drop 2 current-tab ! exit then
  dup [char] 4 = if drop 3 current-tab ! exit then
  dup [char] 5 = if drop 4 current-tab ! exit then
  dup [char] 6 = if drop 5 current-tab ! exit then
  dup 27 = if drop handle-esc exit then
  \ Pass to per-tab handler
  'handle-tab-key @ dup 0= if drop drop else execute then ;

\ ============================================================
\ MAIN LOOP
\ ============================================================

: draw-separator ( -- )
  2 1 term-goto
  fg-cyan
  term-cols @ 0 ?do [char] - emit loop
  attr-reset ;

: draw-screen ( -- )
  cursor-hide
  term-clear
  draw-tab-bar
  draw-separator
  'draw-content @ ?exec
  draw-status-bar ;

: tui-init ( -- )
  detect-size
  tui-raw
  0 quit-flag !
  0 refresh-flag !
  0 current-tab ! ;

: tui-loop ( -- )
  begin
    draw-screen
    key
    handle-key
  quit-flag @ until ;

: tui-run ( -- )
  tui-init
  tui-loop
  tui-cleanup ;
