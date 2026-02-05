/* macho-test.c - Generate minimal Mach-O ARM64 executables
 *
 * Phase 0 reference tool for the Shannon ARM64 port.
 * Produces dynamically-linked executables using LC_MAIN.
 * (macOS ARM64 does NOT support static LC_UNIXTHREAD binaries.)
 *
 * Our code still uses SVC #0x80 for syscalls directly — we link
 * libSystem only because dyld requires it, not because we call it.
 *
 * Layout:
 *   __PAGEZERO  - 4GB null guard
 *   __TEXT      - headers + code (one 16KB page)
 *   __LINKEDIT  - chained fixups, exports trie, symtab, codesig
 *
 * LINKEDIT layout at fileoff=PAGE_SIZE:
 *   [0..55]    chained fixups (dyld requires, even if empty)
 *   [56..103]  exports trie (__mh_execute_header, _main)
 *   [104..135] nlist_64 x 2 (symbol table)
 *   [136..175] string table (40 bytes)
 *   [176..511] padding (codesign appends signature data here)
 *
 * Usage:
 *   cc -o tools/macho-test tools/macho-test.c
 *   ./tools/macho-test exit 42 /tmp/test42
 *   chmod +x /tmp/test42 && codesign -f -s - /tmp/test42 && /tmp/test42; echo $?
 */

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>

/* ================================================================
 * Binary buffer
 * ================================================================ */

#define PAGE_SIZE 0x4000   /* 16KB on Apple Silicon */

static uint8_t buf[PAGE_SIZE * 2];
static int pos = 0;

static void e8(uint8_t v)   { buf[pos++] = v; }
static void e32(uint32_t v) { e8(v); e8(v>>8); e8(v>>16); e8(v>>24); }
static void e64(uint64_t v) { e32((uint32_t)v); e32((uint32_t)(v>>32)); }
static void estr(const char *s, int field_size) {
    int len = (int)strlen(s);
    for (int i = 0; i < field_size; i++)
        e8(i < len ? (uint8_t)s[i] : 0);
}

/* ================================================================
 * ARM64 instruction builders
 * ================================================================ */

static uint32_t arm_movz(int rd, uint16_t imm, int shift) {
    return 0xD2800000 | ((uint32_t)(shift/16) << 21) | ((uint32_t)imm << 5) | (uint32_t)rd;
}
static uint32_t arm_movk(int rd, uint16_t imm, int shift) {
    return 0xF2800000 | ((uint32_t)(shift/16) << 21) | ((uint32_t)imm << 5) | (uint32_t)rd;
}
static uint32_t arm_mul(int rd, int rn, int rm) {
    return 0x9B007C00 | (uint32_t)rd | ((uint32_t)rn << 5) | ((uint32_t)rm << 16);
}
static uint32_t arm_sub_imm(int rd, int rn, uint32_t imm12) {
    return 0xD1000000 | (uint32_t)rd | ((uint32_t)rn << 5) | ((imm12 & 0xFFF) << 10);
}
static uint32_t arm_add_imm(int rd, int rn, uint32_t imm12) {
    return 0x91000000 | (uint32_t)rd | ((uint32_t)rn << 5) | ((imm12 & 0xFFF) << 10);
}
static uint32_t arm_str(int rt, int rn) {
    return 0xF9000000 | (uint32_t)rt | ((uint32_t)rn << 5);
}
static uint32_t arm_svc(uint16_t imm) {
    return 0xD4000001 | ((uint32_t)imm << 5);
}

/* ================================================================
 * LINKEDIT data emitters
 * ================================================================ */

/* Emit chained fixups (56 bytes) — empty, but dyld/codesign require it */
static void emit_chained_fixups(void) {
    int start = pos;
    /* dyld_chained_fixups_header (28 bytes + 4 pad = 32) */
    e32(0);             /* fixups_version */
    e32(32);            /* starts_offset (from start of this data) */
    e32(48);            /* imports_offset */
    e32(48);            /* symbols_offset */
    e32(0);             /* imports_count */
    e32(1);             /* imports_format: DYLD_CHAINED_IMPORT */
    e32(0);             /* symbols_format */
    e32(0);             /* padding to 32 bytes */
    /* dyld_chained_starts_in_image (16 bytes at offset 32) */
    e32(3);             /* seg_count: PAGEZERO, TEXT, LINKEDIT */
    e32(0);             /* seg_info_offset[0]: no fixups */
    e32(0);             /* seg_info_offset[1]: no fixups */
    e32(0);             /* seg_info_offset[2]: no fixups */
    /* pad to 56 bytes */
    e32(0); e32(0);
    if (pos - start != 56) {
        fprintf(stderr, "BUG: chained fixups %d bytes, expected 56\n", pos - start);
        exit(2);
    }
}

/* Emit exports trie (48 bytes) — encodes __mh_execute_header and _main
 *
 * Trie structure:
 *   Root: edge "_" -> N1
 *   N1:   edge "_mh_execute_header" -> N2 (addr=0)
 *         edge "main" -> N3 (addr=CODE_OFFSET=624)
 *
 * Byte layout:
 *   [0]     0x00  root: not terminal
 *   [1]     0x01  root: 1 child
 *   [2-3]   "_\0" root edge
 *   [4]     0x05  child offset -> N1
 *   [5]     0x00  N1: not terminal
 *   [6]     0x02  N1: 2 children
 *   [7-25]  "_mh_execute_header\0" (19 bytes)
 *   [26]    0x21  child offset -> N2 (33)
 *   [27-31] "main\0" (5 bytes)
 *   [32]    0x25  child offset -> N3 (37)
 *   [33]    0x02  N2: terminal_size=2
 *   [34]    0x00  flags=0
 *   [35]    0x00  address=0 (uleb128)
 *   [36]    0x00  N2: 0 children
 *   [37]    0x03  N3: terminal_size=3
 *   [38]    0x00  flags=0
 *   [39-40] 0xF0 0x04  address=624 (uleb128)
 *   [41]    0x00  N3: 0 children
 *   [42-47] padding
 */
static void emit_exports_trie(void) {
    int start = pos;

    /* Root node (trie offset 0) */
    e8(0x00);           /* terminal_size: not a terminal */
    e8(0x01);           /* nchildren: 1 */
    e8('_');            /* edge: "_" */
    e8(0x00);           /* edge null terminator */
    e8(0x05);           /* child offset: 5 */

    /* N1 (trie offset 5) */
    e8(0x00);           /* terminal_size: not a terminal */
    e8(0x02);           /* nchildren: 2 */
    /* child 1 edge: "_mh_execute_header" (18 chars + null = 19 bytes) */
    estr("_mh_execute_header", 18);
    e8(0x00);           /* edge null terminator */
    e8(0x21);           /* child offset: 33 */
    /* child 2 edge: "main" (4 chars + null = 5 bytes) */
    estr("main", 4);
    e8(0x00);           /* edge null terminator */
    e8(0x25);           /* child offset: 37 */

    /* N2 (trie offset 33) — __mh_execute_header at image offset 0 */
    e8(0x02);           /* terminal_size: 2 (flags + addr) */
    e8(0x00);           /* flags: EXPORT_SYMBOL_FLAGS_KIND_REGULAR */
    e8(0x00);           /* address: 0 (uleb128) */
    e8(0x00);           /* nchildren: 0 */

    /* N3 (trie offset 37) — _main at image offset 624 */
    e8(0x03);           /* terminal_size: 3 (flags + 2-byte addr) */
    e8(0x00);           /* flags: EXPORT_SYMBOL_FLAGS_KIND_REGULAR */
    e8(0xF0);           /* address low: (624 & 0x7F) | 0x80 */
    e8(0x04);           /* address high: 624 >> 7 */
    e8(0x00);           /* nchildren: 0 */

    /* Pad to 48 bytes */
    while (pos - start < 48) e8(0);
    if (pos - start != 48) {
        fprintf(stderr, "BUG: exports trie %d bytes, expected 48\n", pos - start);
        exit(2);
    }
}

/* ================================================================
 * Mach-O generation
 *
 * Load commands (11):
 *   0  LC_SEGMENT_64 __PAGEZERO        (72)
 *   1  LC_SEGMENT_64 __TEXT             (152) with __text section
 *   2  LC_SEGMENT_64 __LINKEDIT         (72)
 *   3  LC_LOAD_DYLINKER                (32)  "/usr/lib/dyld"
 *   4  LC_BUILD_VERSION                (32)  platform=macOS
 *   5  LC_MAIN                         (24)  entry offset
 *   6  LC_LOAD_DYLIB                   (56)  "/usr/lib/libSystem.B.dylib"
 *   7  LC_SYMTAB                       (24)  symbol table
 *   8  LC_DYSYMTAB                     (80)  dynamic symbols
 *   9  LC_DYLD_CHAINED_FIXUPS         (16)  empty chained fixups
 *  10  LC_DYLD_EXPORTS_TRIE           (16)  exports trie
 *      + 16 bytes padding (codesign inserts LC_CODE_SIGNATURE here)
 *
 *   sizeofcmds = 72+152+72+32+32+24+56+24+80+16+16 = 576
 *   CODE_OFFSET = 32 + 576 + 16(pad) = 624
 * ================================================================ */

#define HEADER_SIZE      32
#define SIZEOFCMDS       576
#define NCMDS            11
#define CODESIGN_PAD     16   /* codesign inserts LC_CODE_SIGNATURE here */
#define CODE_OFFSET      (HEADER_SIZE + SIZEOFCMDS + CODESIGN_PAD)  /* 624 */

/* LINKEDIT layout: chained_fixups | exports_trie | nlist | strtab | padding */
#define CHAINED_FIX_SIZE 56
#define EXPORTS_SIZE     48
#define NSYMS            2
#define NLIST_SIZE       (NSYMS * 16)                /* 32 */
#define STRTAB_SIZE      40                          /* 27 used + 13 pad */
#define LINKEDIT_DATA    (CHAINED_FIX_SIZE + EXPORTS_SIZE + NLIST_SIZE + STRTAB_SIZE)  /* 176 */
#define LINKEDIT_PAD     336  /* codesign extends LINKEDIT for signature data */
#define LINKEDIT_FSIZE   (LINKEDIT_DATA + LINKEDIT_PAD)  /* 512 */

/* File offsets for LC_SYMTAB */
#define SYMTAB_OFF       (PAGE_SIZE + CHAINED_FIX_SIZE + EXPORTS_SIZE)
#define STRTAB_OFF       (SYMTAB_OFF + NLIST_SIZE)

static void write_macho(const char *outfile, uint32_t *code, int ninsn) {
    uint64_t vmaddr = 0x100000000ULL;
    int code_size = ninsn * 4;

    pos = 0;

    /* === Mach-O Header (32 bytes) === */
    e32(0xFEEDFACF);           /* magic */
    e32(0x0100000C);           /* cputype: ARM64 */
    e32(0x00000000);           /* cpusubtype */
    e32(2);                    /* filetype: MH_EXECUTE */
    e32(NCMDS);
    e32(SIZEOFCMDS);
    e32(0x00200085);           /* flags: MH_PIE | MH_TWOLEVEL | MH_DYLDLINK | MH_NOUNDEFS */
    e32(0);                    /* reserved */

    /* === LC_SEGMENT_64 __PAGEZERO (72 bytes) === */
    e32(0x19); e32(72);        /* cmd, cmdsize */
    estr("__PAGEZERO", 16);
    e64(0); e64(0x100000000ULL); /* vmaddr, vmsize */
    e64(0); e64(0);            /* fileoff, filesize */
    e32(0); e32(0);            /* maxprot, initprot */
    e32(0); e32(0);            /* nsects, flags */

    /* === LC_SEGMENT_64 __TEXT + __text section (152 bytes) === */
    e32(0x19); e32(152);
    estr("__TEXT", 16);
    e64(vmaddr);               /* vmaddr */
    e64(PAGE_SIZE);            /* vmsize: one page */
    e64(0);                    /* fileoff */
    e64(PAGE_SIZE);            /* filesize: entire first page */
    e32(5); e32(5);            /* maxprot=r-x, initprot=r-x */
    e32(1); e32(0);            /* nsects=1, flags */
    /* __text section (80 bytes) */
    estr("__text", 16);
    estr("__TEXT", 16);
    e64(vmaddr + CODE_OFFSET); /* addr */
    e64((uint64_t)code_size);  /* size */
    e32(CODE_OFFSET);          /* offset */
    e32(2);                    /* align: 4 bytes */
    e32(0); e32(0);            /* reloff, nreloc */
    e32(0x80000400);           /* flags: PURE + SOME_INSTRUCTIONS */
    e32(0); e32(0); e32(0);    /* reserved */

    /* === LC_SEGMENT_64 __LINKEDIT (72 bytes) === */
    e32(0x19); e32(72);
    estr("__LINKEDIT", 16);
    e64(vmaddr + PAGE_SIZE);   /* vmaddr: next page */
    e64(PAGE_SIZE);            /* vmsize */
    e64(PAGE_SIZE);            /* fileoff: after __TEXT page */
    e64(LINKEDIT_FSIZE);       /* filesize: 512 bytes of actual data */
    e32(1); e32(1);            /* maxprot=r, initprot=r */
    e32(0); e32(0);            /* nsects, flags */

    /* === LC_LOAD_DYLINKER (32 bytes) === */
    e32(0x0E); e32(32);       /* cmd=LC_LOAD_DYLINKER, cmdsize */
    e32(12);                   /* name offset (from start of cmd) */
    estr("/usr/lib/dyld", 20); /* 14 chars + null, padded to 20 */

    /* === LC_BUILD_VERSION (32 bytes) === */
    e32(0x32); e32(32);       /* cmd=LC_BUILD_VERSION */
    e32(1);                    /* platform: MACOS */
    e32(0x000F0000);           /* minos: 15.0.0 */
    e32(0x000F0000);           /* sdk: 15.0.0 */
    e32(1);                    /* ntools */
    e32(3);                    /* tool: LD */
    e32(0x04CE0100);           /* tool version */

    /* === LC_MAIN (24 bytes) === */
    e32(0x80000028); e32(24); /* cmd=LC_MAIN */
    e64((uint64_t)CODE_OFFSET); /* entryoff */
    e64(0);                    /* stacksize: default */

    /* === LC_LOAD_DYLIB (56 bytes) === */
    e32(0x0C); e32(56);       /* cmd=LC_LOAD_DYLIB */
    e32(24);                   /* name offset */
    e32(2);                    /* timestamp */
    e32(0x054C0000);           /* current_version */
    e32(0x00010000);           /* compat_version */
    estr("/usr/lib/libSystem.B.dylib", 32);

    /* === LC_SYMTAB (24 bytes) === */
    e32(0x02); e32(24);       /* cmd=LC_SYMTAB */
    e32(SYMTAB_OFF);           /* symoff */
    e32(NSYMS);                /* nsyms */
    e32(STRTAB_OFF);           /* stroff */
    e32(STRTAB_SIZE);          /* strsize */

    /* === LC_DYSYMTAB (80 bytes) === */
    e32(0x0B); e32(80);       /* cmd=LC_DYSYMTAB */
    e32(0);                    /* ilocalsym */
    e32(0);                    /* nlocalsym */
    e32(0);                    /* iextdefsym */
    e32(NSYMS);                /* nextdefsym */
    e32(NSYMS);                /* iundefsym */
    e32(0);                    /* nundefsym */
    e32(0); e32(0);            /* tocoff, ntoc */
    e32(0); e32(0);            /* modtaboff, nmodtab */
    e32(0); e32(0);            /* extrefsymoff, nextrefsyms */
    e32(0); e32(0);            /* indirectsymoff, nindirectsyms */
    e32(0); e32(0);            /* extreloff, nextrel */
    e32(0); e32(0);            /* locreloff, nlocrel */

    /* === LC_DYLD_CHAINED_FIXUPS (16 bytes) === */
    e32(0x80000034); e32(16); /* cmd, cmdsize */
    e32(PAGE_SIZE);            /* dataoff: start of LINKEDIT */
    e32(CHAINED_FIX_SIZE);     /* datasize: 56 */

    /* === LC_DYLD_EXPORTS_TRIE (16 bytes) === */
    e32(0x80000033); e32(16); /* cmd, cmdsize */
    e32(PAGE_SIZE + CHAINED_FIX_SIZE); /* dataoff */
    e32(EXPORTS_SIZE);         /* datasize: 48 */

    /* 16 bytes padding — codesign inserts LC_CODE_SIGNATURE here */
    for (int i = 0; i < CODESIGN_PAD; i++) e8(0);

    /* Verify header + load commands + padding = CODE_OFFSET */
    if (pos != CODE_OFFSET) {
        fprintf(stderr, "BUG: pos=%d expected=%d (diff=%d)\n",
                pos, CODE_OFFSET, pos - CODE_OFFSET);
        exit(2);
    }

    /* === Code === */
    for (int i = 0; i < ninsn; i++)
        e32(code[i]);

    /* Pad to page boundary */
    while (pos < PAGE_SIZE) e8(0);

    /* === LINKEDIT content === */

    /* Chained fixups (56 bytes) */
    emit_chained_fixups();

    /* Exports trie (48 bytes) */
    emit_exports_trie();

    /* nlist_64 for __mh_execute_header (16 bytes) */
    e32(1);                     /* n_strx: offset into strtab */
    e8(0x0F); e8(1);           /* n_type=N_SECT|N_EXT, n_sect=1 (__text) */
    e8(0x10); e8(0x00);        /* n_desc=REFERENCED_DYNAMICALLY */
    e64(vmaddr);                /* n_value: start of __TEXT */

    /* nlist_64 for _main (16 bytes) */
    e32(21);                    /* n_strx: offset into strtab */
    e8(0x0F); e8(1);           /* n_type=N_SECT|N_EXT, n_sect=1 */
    e8(0x00); e8(0x00);        /* n_desc=0 */
    e64(vmaddr + CODE_OFFSET);  /* n_value: entry point */

    /* String table (40 bytes: 27 used + 13 pad) */
    e8(0);                      /* index 0: empty string */
    estr("__mh_execute_header", 20); /* index 1, 20 bytes incl null */
    estr("_main", 6);          /* index 21, 6 bytes incl null */
    while (pos < (int)(STRTAB_OFF + STRTAB_SIZE)) e8(0);

    /* Padding (codesign extends LINKEDIT for signature data) */
    while (pos < (int)(PAGE_SIZE + LINKEDIT_FSIZE)) e8(0);

    /* Write */
    FILE *f = fopen(outfile, "wb");
    if (!f) { perror("fopen"); exit(1); }
    fwrite(buf, 1, pos, f);
    fclose(f);

    fprintf(stderr, "Wrote %d bytes (%d insns at offset %d)\n", pos, ninsn, CODE_OFFSET);
}

/* ================================================================
 * Test modes
 * ================================================================ */

int main(int argc, char **argv) {
    if (argc < 3) {
        fprintf(stderr, "Usage: macho-test <mode> <arg> [outfile]\n");
        fprintf(stderr, "  exit <code>  - exit with given code\n");
        fprintf(stderr, "  arith 0      - compute 6*7, exit 42\n");
        fprintf(stderr, "  hello 0      - print Hello, exit 0\n");
        return 1;
    }

    const char *mode = argv[1];
    const char *outfile = argc > 3 ? argv[3] : "/tmp/test-arm64";
    uint32_t code[256];
    int n = 0;

    if (strcmp(mode, "exit") == 0) {
        int ec = atoi(argv[2]);
        code[n++] = arm_movz(0, (uint16_t)(ec & 0xFFFF), 0);
        code[n++] = arm_movz(16, 1, 0);
        code[n++] = arm_svc(0x80);
    }
    else if (strcmp(mode, "arith") == 0) {
        code[n++] = arm_movz(0, 6, 0);
        code[n++] = arm_movz(1, 7, 0);
        code[n++] = arm_mul(0, 0, 1);
        code[n++] = arm_movz(16, 1, 0);
        code[n++] = arm_svc(0x80);
    }
    else if (strcmp(mode, "hello") == 0) {
        code[n++] = arm_sub_imm(31, 31, 16);
        code[n++] = arm_movz(9, 'H' | ('e' << 8), 0);
        code[n++] = arm_movk(9, 'l' | ('l' << 8), 16);
        code[n++] = arm_movk(9, 'o' | ('\n' << 8), 32);
        code[n++] = arm_str(9, 31);
        code[n++] = arm_movz(0, 1, 0);
        code[n++] = arm_add_imm(1, 31, 0);
        code[n++] = arm_movz(2, 6, 0);
        code[n++] = arm_movz(16, 4, 0);
        code[n++] = arm_svc(0x80);
        code[n++] = arm_movz(0, 0, 0);
        code[n++] = arm_movz(16, 1, 0);
        code[n++] = arm_svc(0x80);
    }
    else {
        fprintf(stderr, "Unknown mode: %s\n", mode);
        return 1;
    }

    write_macho(outfile, code, n);
    return 0;
}
