\ macho.fs - Mach-O ARM64 binary format (Shannon Layer 2)
\ Depends on: asm.fs (code-buf, code-pos for copy-code)
\
\ Emits a complete Mach-O dynamically-linked executable:
\   11 load commands, LC_MAIN entry, 16KB pages, codesign-ready.
\ Reference: tools/macho-test.c (Phase 0, all tests passing)

\ ============================================================
\ CONSTANTS
\ ============================================================

variable main-entry   \ code-pos where 'main' starts (set by compile-colon)

$4000 constant PAGE-SIZE
32 constant HEADER-SIZE
576 constant SIZEOFCMDS
11 constant NCMDS
16 constant CODESIGN-PAD
HEADER-SIZE SIZEOFCMDS + CODESIGN-PAD + constant CODE-OFFSET  \ 624
1 32 lshift constant VMADDR  \ 0x100000000

\ LINKEDIT layout
56 constant CHAINED-FIX-SIZE
48 constant EXPORTS-SIZE
2 constant NSYMS
NSYMS 16 * constant NLIST-SIZE  \ 32
40 constant STRTAB-SIZE
PAGE-SIZE CHAINED-FIX-SIZE + EXPORTS-SIZE + constant SYMTAB-OFF
SYMTAB-OFF NLIST-SIZE + constant STRTAB-OFF
512 constant LINKEDIT-FSIZE

\ ============================================================
\ FILE BUFFER
\ ============================================================

65536 constant FILE-SIZE
create file-buf FILE-SIZE allot
variable fpos   0 fpos !

: f, ( b -- ) file-buf fpos @ + c!  1 fpos +! ;
: fd, ( u -- ) dup f, 8 rshift dup f, 8 rshift dup f, 8 rshift f, ;
: fq, ( u -- ) dup fd, 32 rshift fd, ;
: fpad-to ( target -- ) begin dup fpos @ > while 0 f, repeat drop ;

: fstr ( c-addr u fieldsize -- )
  0 do
    dup i > if over i + c@ f, else 0 f, then
  loop 2drop ;

: ftype ( c-addr u -- )
  dup 0= if 2drop exit then
  over + swap do i c@ f, loop ;

\ ============================================================
\ MACH-O HEADER (32 bytes)
\ ============================================================

variable code-sz

: write-mach-header ( -- )
  $FEEDFACF fd,     \ magic
  $0100000C fd,     \ cputype: ARM64
  0 fd,             \ cpusubtype
  2 fd,             \ filetype: MH_EXECUTE
  NCMDS fd,         \ ncmds
  SIZEOFCMDS fd,    \ sizeofcmds
  $00200085 fd,     \ flags: PIE|TWOLEVEL|DYLDLINK|NOUNDEFS
  0 fd, ;           \ reserved

\ ============================================================
\ LOAD COMMANDS (576 bytes total)
\ ============================================================

: write-pagezero ( -- )  \ 72 bytes
  $19 fd, 72 fd,
  s" __PAGEZERO" 16 fstr
  0 fq, VMADDR fq,
  0 fq, 0 fq,
  0 fd, 0 fd,
  0 fd, 0 fd, ;

: write-text-segment ( -- )  \ 152 bytes (72 + 80 section)
  $19 fd, 152 fd,
  s" __TEXT" 16 fstr
  VMADDR fq,
  PAGE-SIZE fq,
  0 fq,
  PAGE-SIZE fq,
  5 fd, 5 fd,
  1 fd, 0 fd,
  s" __text" 16 fstr
  s" __TEXT" 16 fstr
  VMADDR CODE-OFFSET + fq,
  code-sz @ fq,
  CODE-OFFSET fd,
  2 fd,
  0 fd, 0 fd,
  $80000400 fd,
  0 fd, 0 fd, 0 fd, ;

: write-linkedit ( -- )  \ 72 bytes
  $19 fd, 72 fd,
  s" __LINKEDIT" 16 fstr
  VMADDR PAGE-SIZE + fq,
  PAGE-SIZE fq,
  PAGE-SIZE fq,
  LINKEDIT-FSIZE fq,
  1 fd, 1 fd,
  0 fd, 0 fd, ;

: write-dylinker ( -- )  \ 32 bytes
  $0E fd, 32 fd,
  12 fd,
  s" /usr/lib/dyld" 20 fstr ;

: write-build-version ( -- )  \ 32 bytes
  $32 fd, 32 fd,
  1 fd,
  $000F0000 fd,
  $000F0000 fd,
  1 fd,
  3 fd,
  $04CE0100 fd, ;

: write-lc-main ( -- )  \ 24 bytes
  $80000028 fd, 24 fd,
  CODE-OFFSET main-entry @ + fq,    \ entry = CODE-OFFSET + main-entry
  0 fq, ;

: write-load-dylib ( -- )  \ 56 bytes
  $0C fd, 56 fd,
  24 fd,
  2 fd,
  $054C0000 fd,
  $00010000 fd,
  s" /usr/lib/libSystem.B.dylib" 32 fstr ;

: write-symtab ( -- )  \ 24 bytes
  $02 fd, 24 fd,
  SYMTAB-OFF fd,
  NSYMS fd,
  STRTAB-OFF fd,
  STRTAB-SIZE fd, ;

: write-dysymtab ( -- )  \ 80 bytes
  $0B fd, 80 fd,
  0 fd, 0 fd,
  0 fd, NSYMS fd,
  NSYMS fd, 0 fd,
  0 fd, 0 fd,
  0 fd, 0 fd,
  0 fd, 0 fd,
  0 fd, 0 fd,
  0 fd, 0 fd,
  0 fd, 0 fd, ;

: write-chained-fixups-lc ( -- )  \ 16 bytes
  $80000034 fd, 16 fd,
  PAGE-SIZE fd,
  CHAINED-FIX-SIZE fd, ;

: write-exports-trie-lc ( -- )  \ 16 bytes
  $80000033 fd, 16 fd,
  PAGE-SIZE CHAINED-FIX-SIZE + fd,
  EXPORTS-SIZE fd, ;

\ ============================================================
\ LINKEDIT DATA
\ ============================================================

: write-chained-fixups ( -- )  \ 56 bytes
  0 fd, 32 fd, 48 fd, 48 fd,
  0 fd, 1 fd, 0 fd, 0 fd,
  3 fd, 0 fd, 0 fd, 0 fd,
  0 fd, 0 fd, ;

: write-exports-trie ( -- )  \ 48 bytes
  0 f, 1 f,
  [char] _ f, 0 f,
  5 f,
  0 f, 2 f,
  s" _mh_execute_header" ftype
  0 f, $21 f,
  s" main" ftype
  0 f, $25 f,
  2 f, 0 f, 0 f, 0 f,
  3 f, 0 f, $F0 f, 4 f, 0 f,
  PAGE-SIZE CHAINED-FIX-SIZE + EXPORTS-SIZE + fpad-to ;

: write-nlist ( -- )  \ 32 bytes (2 x 16)
  1 fd, $0F f, 1 f, $10 f, 0 f,
  VMADDR fq,
  21 fd, $0F f, 1 f, 0 f, 0 f,
  VMADDR CODE-OFFSET + fq, ;

: write-strtab ( -- )  \ 40 bytes
  0 f,
  s" __mh_execute_header" 20 fstr
  s" _main" 6 fstr
  STRTAB-OFF STRTAB-SIZE + fpad-to ;

\ ============================================================
\ BUILD AND SAVE
\ ============================================================

: copy-code ( -- )
  code-pos @ 0 ?do code-buf i + c@ f, loop ;

: build-macho ( -- )
  0 fpos !
  code-pos @ code-sz !
  write-mach-header
  write-pagezero
  write-text-segment
  write-linkedit
  write-dylinker
  write-build-version
  write-lc-main
  write-load-dylib
  write-symtab
  write-dysymtab
  write-chained-fixups-lc
  write-exports-trie-lc
  CODE-OFFSET fpad-to
  copy-code
  PAGE-SIZE fpad-to
  write-chained-fixups
  write-exports-trie
  write-nlist
  write-strtab
  PAGE-SIZE LINKEDIT-FSIZE + fpad-to ;

256 constant CMD-BUF-SIZE
create cmd-buf CMD-BUF-SIZE allot

: save-binary ( c-addr u -- )
  2dup                                \ ( fn-addr fn-u fn-addr fn-u )
  w/o create-file throw
  >r
  file-buf fpos @ r@ write-file throw
  r> close-file throw                 \ ( fn-addr fn-u )
  \ Build and run: chmod +x <filename>
  cmd-buf CMD-BUF-SIZE 0 fill
  s" chmod +x " cmd-buf swap move     \ copy "chmod +x " to cmd-buf
  2dup cmd-buf 9 + swap move          \ copy filename after it
  cmd-buf CMD-BUF-SIZE system         \ run chmod +x <fn>
  \ Build and run: codesign -s - <filename>
  cmd-buf CMD-BUF-SIZE 0 fill
  s" codesign -s - " cmd-buf swap move
  cmd-buf 14 + swap move              \ ( ) - filename consumed
  cmd-buf CMD-BUF-SIZE system ;       \ run codesign -s - <fn>
