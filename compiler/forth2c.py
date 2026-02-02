#!/usr/bin/env python3
"""Minimal Forth-to-C translator for benchmarking sixth.fs against GCC."""
import sys

def tokenize(text):
    toks = []
    i = 0
    while i < len(text):
        while i < len(text) and text[i] in ' \t\n\r':
            i += 1
        if i >= len(text):
            break
        if text[i] == '\\' and (i == 0 or text[i-1] in ' \t\n\r'):
            while i < len(text) and text[i] != '\n':
                i += 1
            continue
        if text[i] == '(' and i+1 < len(text) and text[i+1] in ' \t\n\r':
            depth = 1
            i += 1
            while i < len(text) and depth > 0:
                if text[i] == ')': depth -= 1
                elif text[i] == '(': depth += 1
                i += 1
            continue
        start = i
        while i < len(text) and text[i] not in ' \t\n\r':
            i += 1
        toks.append(text[start:i])
    return toks

def sanitize(name):
    return (name.replace('-','_').replace('?','_q').replace('!','_x')
            .replace('/','_d').replace('<','_lt').replace('>','_gt')
            .replace('=','_eq').replace('+','_plus').replace('*','_star'))

def translate(src_file, dst_file):
    src = open(src_file).read()
    tokens = tokenize(src)

    # Pass 1: collect word names
    words = []
    i = 0
    while i < len(tokens):
        if tokens[i] == ':' and i+1 < len(tokens):
            words.append(tokens[i+1])
            i += 2
            while i < len(tokens) and tokens[i] != ';':
                i += 1
        i += 1

    do_depth = 0

    def emit_tok(tok, cur_word):
        nonlocal do_depth
        m = {
            '+': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a+b; }',
            '-': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a-b; }',
            '*': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a*b; }',
            '/': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a/b; }',
            'mod': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a%b; }',
            '/mod': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a%b; s[++sp]=a/b; }',
            'and': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a&b; }',
            'or': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a|b; }',
            'xor': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=a^b; }',
            'invert': 's[sp] = ~s[sp];',
            'negate': 's[sp] = -s[sp];',
            'abs': 's[sp] = s[sp]<0 ? -s[sp] : s[sp];',
            '1+': 's[sp]++;', '1-': 's[sp]--;',
            '2+': 's[sp]+=2;', '2-': 's[sp]-=2;',
            '2*': 's[sp]*=2;', '2/': 's[sp]/=2;',
            '<': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a<b)?-1:0; }',
            '>': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a>b)?-1:0; }',
            '=': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a==b)?-1:0; }',
            '<>': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a!=b)?-1:0; }',
            '<=': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a<=b)?-1:0; }',
            '>=': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a>=b)?-1:0; }',
            '0<': 's[sp] = (s[sp]<0)?-1:0;',
            '0>': 's[sp] = (s[sp]>0)?-1:0;',
            '0=': 's[sp] = (s[sp]==0)?-1:0;',
            'max': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a>b)?a:b; }',
            'min': '{ int64_t b=s[sp--], a=s[sp]; s[sp]=(a<b)?a:b; }',
            'dup': 's[sp+1]=s[sp]; sp++;',
            'drop': 'sp--;',
            'swap': '{ int64_t t=s[sp]; s[sp]=s[sp-1]; s[sp-1]=t; }',
            'over': 's[sp+1]=s[sp-1]; sp++;',
            'rot': '{ int64_t c=s[sp],b=s[sp-1],a=s[sp-2]; s[sp-2]=b; s[sp-1]=c; s[sp]=a; }',
            '-rot': '{ int64_t c=s[sp],b=s[sp-1],a=s[sp-2]; s[sp-2]=c; s[sp-1]=a; s[sp]=b; }',
            'nip': 's[sp-1]=s[sp]; sp--;',
            'tuck': '{ int64_t t=s[sp]; s[sp]=s[sp-1]; s[sp-1]=t; s[++sp]=t; }',
            '2dup': 's[sp+1]=s[sp-1]; s[sp+2]=s[sp]; sp+=2;',
            '2drop': 'sp-=2;',
            'depth': 's[sp+1]=sp+1; sp++;',
            '.': 'printf("%ld ", s[sp--]);',
            'cr': 'printf("\\n");',
            'emit': 'putchar((int)s[sp--]);',
            'exit': 'return sp;',
            'if': '{ int64_t cond=s[sp--]; if(cond) {',
            'else': '} else {',
            'then': '}}',
            'begin': 'for(;;) {',
            'until': 'if(s[sp--]) break; }',
            'while': 'if(!s[sp--]) break;',
            'repeat': '}',
            'again': '}',
        }
        if tok in m:
            return m[tok]
        if tok == 'do':
            r = '{ int64_t _i%d=s[sp--], _lim%d=s[sp--]; for(;;) {' % (do_depth, do_depth)
            do_depth += 1
            return r
        if tok == 'loop':
            do_depth -= 1
            return 'if(++_i%d >= _lim%d) break; }}' % (do_depth, do_depth)
        if tok == '+loop':
            do_depth -= 1
            return '_i%d += s[sp--]; if(_i%d >= _lim%d) break; }}' % (do_depth, do_depth, do_depth)
        if tok == 'i':
            return 's[++sp] = _i%d;' % (do_depth - 1)
        if tok == 'j':
            return 's[++sp] = _i%d;' % (do_depth - 2)
        if tok == 'recurse' or tok == cur_word:
            return 'sp = call_%s(s, sp);' % sanitize(cur_word)
        if tok in words:
            return 'sp = call_%s(s, sp);' % sanitize(tok)
        # Number
        try:
            if tok.startswith('$'):
                val = int(tok[1:], 16)
            elif tok.startswith('0x'):
                val = int(tok, 16)
            else:
                val = int(tok)
            return 's[++sp] = %dLL;' % val
        except ValueError:
            pass
        return '/* UNKNOWN: %s */' % tok

    out = ['#include <stdio.h>', '#include <stdint.h>', 'int64_t s[1024];', '']
    for w in words:
        if w != 'main':
            out.append('static int call_%s(int64_t *s, int sp);' % sanitize(w))
    out.append('')

    i = 0
    while i < len(tokens):
        if tokens[i] == ':' and i+1 < len(tokens):
            wname = tokens[i+1]
            i += 2
            do_depth = 0
            if wname == 'main':
                out.append('int main(void) {')
                out.append('  int sp = -1;')
            else:
                out.append('static int call_%s(int64_t *s, int sp) {' % sanitize(wname))
            while i < len(tokens) and tokens[i] != ';':
                c = emit_tok(tokens[i], wname)
                if c:
                    out.append('  ' + c)
                i += 1
            if wname == 'main':
                out.append('  return 0;')
            else:
                out.append('  return sp;')
            out.append('}')
            out.append('')
        i += 1

    open(dst_file, 'w').write('\n'.join(out) + '\n')

if __name__ == '__main__':
    if len(sys.argv) != 3:
        print("Usage: forth2c.py <input.fs> <output.c>", file=sys.stderr)
        sys.exit(1)
    translate(sys.argv[1], sys.argv[2])
