\ elf.fs - ELF64 binary generation for x86-64 Linux
\ Part of the Shannon Architecture refactoring
\
\ Generates a minimal static ELF64 executable with:
\ - ELF header (64 bytes)
\ - Code segment program header (56 bytes) - RX at 0x400000
\ - Data segment program header (56 bytes) - RW at 0x800000
\ Total header size: 176 bytes

\ ============================================================
\ ELF BUFFER
\ ============================================================

create elf-buf 256 allot
variable elf-pos  0 elf-pos !

\ ============================================================
\ ELF EMISSION WORDS
\ ============================================================

: e,  ( b -- )        elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, ( word -- )     dup e, 8 rshift e, ;
: e4, ( dword -- )    dup e2, 16 rshift e2, ;
: e8, ( qword -- )    dup e4, 32 rshift e4, ;

\ ============================================================
\ ELF HEADER GENERATION
\ ============================================================
\ Requires: code-pos (from code generation module)

: elf-header ( -- )
  0 elf-pos !

  \ --- ELF64 Header (64 bytes) ---

  \ e_ident[16]: ELF magic and identification
  $7f e, 69 e, 76 e, 70 e,     \ 0x00: magic "\x7fELF"
  2 e,                          \ 0x04: EI_CLASS = ELFCLASS64
  1 e,                          \ 0x05: EI_DATA = ELFDATA2LSB (little-endian)
  1 e,                          \ 0x06: EI_VERSION = EV_CURRENT
  0 e,                          \ 0x07: EI_OSABI = ELFOSABI_NONE
  0 e8,                         \ 0x08: EI_PAD (8 bytes padding)

  \ ELF header fields
  2 e2,                         \ 0x10: e_type = ET_EXEC (executable)
  $3e e2,                       \ 0x12: e_machine = EM_X86_64
  1 e4,                         \ 0x14: e_version = EV_CURRENT
  $4000B0 e8,                   \ 0x18: e_entry = 0x400000 + 176 (after headers)
  64 e8,                        \ 0x20: e_phoff = 64 (program headers follow ELF header)
  0 e8,                         \ 0x28: e_shoff = 0 (no section headers)
  0 e4,                         \ 0x30: e_flags = 0
  64 e2,                        \ 0x34: e_ehsize = 64 (ELF header size)
  56 e2,                        \ 0x36: e_phentsize = 56 (program header entry size)
  2 e2,                         \ 0x38: e_phnum = 2 (two program headers)
  0 e2,                         \ 0x3A: e_shentsize = 0
  0 e2,                         \ 0x3C: e_shnum = 0
  0 e2,                         \ 0x3E: e_shstrndx = 0

  \ --- Program Header 1: Code Segment (56 bytes) ---
  \ Maps code at 0x400000 with RX permissions

  1 e4,                         \ 0x40: p_type = PT_LOAD
  5 e4,                         \ 0x44: p_flags = PF_R | PF_X (read + execute)
  0 e8,                         \ 0x48: p_offset = 0 (start of file)
  $400000 e8,                   \ 0x50: p_vaddr = 0x400000
  $400000 e8,                   \ 0x58: p_paddr = 0x400000
  176 code-pos @ +              \ p_filesz = header + code
  dup e8,                       \ 0x60: p_filesz
  e8,                           \ 0x68: p_memsz = same as filesz
  $1000 e8,                     \ 0x70: p_align = 4096

  \ --- Program Header 2: Data Segment (56 bytes) ---
  \ Maps data at 0x800000 with RW permissions (2MB region)

  1 e4,                         \ 0x78: p_type = PT_LOAD
  6 e4,                         \ 0x7C: p_flags = PF_R | PF_W (read + write)
  0 e8,                         \ 0x80: p_offset = 0
  $800000 e8,                   \ 0x88: p_vaddr = 0x800000
  $800000 e8,                   \ 0x90: p_paddr = 0x800000
  0 e8,                         \ 0x98: p_filesz = 0 (BSS-like, not in file)
  $200000 e8,                   \ 0xA0: p_memsz = 2MB (zeroed by kernel)
  $1000 e8, ;                   \ 0xA8: p_align = 4096

\ ============================================================
\ ELF OUTPUT
\ ============================================================
\ Requires: code-buf, code-pos, create-file, write-file, close-file, throw, w/o

: write-elf ( filename-addr filename-len -- )
  w/o create-file throw >r
  elf-buf 176 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;
