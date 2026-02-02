#!/bin/bash
# run-bench.sh - Benchmark sixth.fs vs GCC at -O0/-O1/-O2/-O3
# Usage: cd fifth && bash compiler/run-bench.sh
# Output: BENCHMARK01.md

set -euo pipefail

TESTDIR="compiler/tests"
TMPDIR="/tmp/fb"
OUTFILE="BENCHMARK01.md"
NOTESFILE="compiler/bench-notes.txt"

rm -rf "$TMPDIR"
mkdir -p "$TMPDIR"

# Load notes into associative array
declare -A NOTES
if [ -f "$NOTESFILE" ]; then
    while IFS=$'\t' read -r key val; do
        [[ "$key" =~ ^#.*$ || -z "$key" ]] && continue
        NOTES["$key"]="$val"
    done < "$NOTESFILE"
fi

# Counters
yes=0; no=0; nt=0; cfail=0; both=0; tffail=0; total=0

# Timing accumulators
tf_comp_sum=0; tf_run_sum=0; g2_comp_sum=0; g2_run_sum=0
tf_comp_n=0; tf_run_n=0; g2_comp_n=0; g2_run_n=0

ns() { date +%s%N; }

# Forth-to-C translator
translate_forth_to_c() {
    local src="$1" dst="$2"
    python3 compiler/forth2c.py "$src" "$dst" 2>/dev/null
}

_DEAD_CODE_START_() { :; }  # marker for removed inline translator
: << 'REMOVED_INLINE_TRANSLATOR'
import sys, re

src = open('$src').read()
lines = src.split('\n')

# Collect user-defined words
words = []
i = 0
tokens = src.split()
while i < len(tokens):
    if tokens[i] == ':' and i+1 < len(tokens):
        words.append(tokens[i+1])
    if tokens[i] == '\\\\':
        # skip to end of line - find position in original
        break
    i += 1

# Re-tokenize properly (handle backslash comments and parens)
def tokenize(text):
    toks = []
    i = 0
    while i < len(text):
        while i < len(text) and text[i] in ' \t\n\r':
            i += 1
        if i >= len(text):
            break
        if text[i] == '\\\\' and (i == 0 or text[i-1] in ' \t\n\r'):
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

tokens = tokenize(src)

# Collect words (pass 1)
words = []
i = 0
while i < len(tokens):
    if tokens[i] == ':' and i+1 < len(tokens):
        words.append(tokens[i+1])
        i += 2
        while i < len(tokens) and tokens[i] != ';':
            i += 1
    i += 1

def sanitize(name):
    return name.replace('-','_').replace('?','_q').replace('!','_x').replace('/','_d').replace('<','_lt').replace('>','_gt').replace('=','_eq').replace('+','_plus').replace('*','_star')

do_depth = 0

def emit_tok(tok, cur_word):
    global do_depth
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
    if tok == '.':
        return 'printf(\"%ld \", s[sp--]);'
    if tok == 'cr':
        return 'printf(\"\\n\");'
    if tok == 'emit':
        return 'putchar((int)s[sp--]);'
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
    # Number?
    try:
        if tok.startswith('\$') or tok.startswith('0x'):
            val = int(tok.replace('\$','0x'), 16)
        else:
            val = int(tok)
        return 's[++sp] = %dLL;' % val
    except:
        pass
    return '/* UNKNOWN: %s */' % tok

out = []
out.append('#include <stdio.h>')
out.append('#include <stdint.h>')
out.append('int64_t s[1024];')
out.append('')

# Forward declarations
for w in words:
    if w != 'main':
        out.append('static int call_%s(int64_t *s, int sp);' % sanitize(w))
out.append('')

# Pass 2: emit code
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

REMOVED_INLINE_TRANSLATOR

# Format milliseconds from nanoseconds
fmt_ms() {
    local ns="$1"
    if [ "$ns" -lt 0 ] 2>/dev/null; then
        echo -n "FAIL"
        return
    fi
    local ms=$((ns / 1000000))
    local frac=$(( (ns / 1000 % 1000) ))
    printf "%d.%03d" "$ms" "$frac"
}

# Format ratio
fmt_ratio() {
    local tf_ns="$1" gcc_ns="$2"
    if [ "$tf_ns" -lt 0 ] 2>/dev/null || [ "$gcc_ns" -le 0 ] 2>/dev/null; then
        echo -n "n/a"
        return
    fi
    local pct=$((tf_ns * 100 / gcc_ns))
    local whole=$((pct / 100))
    local frac=$((pct % 100))
    printf "%d.%02dx" "$whole" "$frac"
}

# Header
{
    echo "# Sixth Compiler Benchmark Results"
    echo ""
    echo "Date: $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""
} > "$OUTFILE"

# Count tests
test_count=$(ls "$TESTDIR"/[0-9]*.fs 2>/dev/null | wc -l)
echo "Tests: $test_count" >> "$OUTFILE"
echo "" >> "$OUTFILE"

echo "| # | Test | Correct | tf comp | gcc-O0 comp | gcc-O1 comp | gcc-O2 comp | gcc-O3 comp | tf run | gcc-O0 run | gcc-O1 run | gcc-O2 run | gcc-O3 run | tf/gcc-O2 | Notes |" >> "$OUTFILE"
echo "|---|------|---------|---------|-------------|-------------|-------------|-------------|--------|------------|------------|------------|------------|-----------|-------|" >> "$OUTFILE"

echo "Benchmarking $test_count tests..."

for testfile in "$TESTDIR"/[0-9]*.fs; do
    total=$((total + 1))
    name=$(basename "$testfile" .fs)

    # Init timings
    tf_comp=-1; tf_run=-1
    g0_comp=-1; g1_comp=-1; g2_comp=-1; g3_comp=-1
    g0_run=-1; g1_run=-1; g2_run=-1; g3_run=-1
    tf_out=""; gcc_out=""

    # 1. Compile with sixth.fs
    t0=$(ns)
    if ./sixth compiler/sixth.fs "$testfile" "$TMPDIR/tf-out" >/dev/null 2>&1; then
        t1=$(ns)
        tf_comp=$((t1 - t0))
    fi

    # 2. Translate to C and compile with GCC
    if translate_forth_to_c "$testfile" "$TMPDIR/test.c"; then
        for opt in 0 1 2 3; do
            t0=$(ns)
            if gcc -O${opt} -o "$TMPDIR/g${opt}" "$TMPDIR/test.c" >/dev/null 2>&1; then
                t1=$(ns)
                eval "g${opt}_comp=$((t1 - t0))"
            fi
        done
    fi

    # 3. Run tf binary
    if [ "$tf_comp" -ge 0 ]; then
        t0=$(ns)
        if tf_out=$(timeout 5 "$TMPDIR/tf-out" 2>&1); then
            t1=$(ns)
            tf_run=$((t1 - t0))
        fi
    fi

    # 4. Run GCC-O0 binary (reference output)
    if [ "$g0_comp" -ge 0 ]; then
        t0=$(ns)
        if gcc_out=$(timeout 5 "$TMPDIR/g0" 2>&1); then
            t1=$(ns)
            g0_run=$((t1 - t0))
        fi
    fi

    # 5. Run other GCC binaries
    for opt in 1 2 3; do
        eval "comp_time=\$g${opt}_comp"
        if [ "$comp_time" -ge 0 ] 2>/dev/null; then
            t0=$(ns)
            if timeout 5 "$TMPDIR/g${opt}" >/dev/null 2>&1; then
                t1=$(ns)
                eval "g${opt}_run=$((t1 - t0))"
            fi
        fi
    done

    # 6. Strip trailing whitespace for comparison
    tf_out_stripped=$(echo "$tf_out" | sed 's/[[:space:]]*$//')
    gcc_out_stripped=$(echo "$gcc_out" | sed 's/[[:space:]]*$//')

    # 7. Determine correctness
    if [ -n "$tf_out_stripped" ] && [ -n "$gcc_out_stripped" ]; then
        if [ "$tf_out_stripped" = "$gcc_out_stripped" ]; then
            correct="YES"; yes=$((yes + 1))
        else
            correct="NO"; no=$((no + 1))
        fi
    elif [ -z "$tf_out_stripped" ] && [ -z "$gcc_out_stripped" ]; then
        correct="BOTH"; both=$((both + 1))
    elif [ -n "$tf_out_stripped" ]; then
        correct="C-FAIL"; cfail=$((cfail + 1))
    else
        correct="TF-FAIL"; tffail=$((tffail + 1))
    fi

    # 8. Accumulate stats
    if [ "$tf_comp" -ge 0 ]; then tf_comp_sum=$((tf_comp_sum + tf_comp)); tf_comp_n=$((tf_comp_n + 1)); fi
    if [ "$tf_run" -ge 0 ]; then tf_run_sum=$((tf_run_sum + tf_run)); tf_run_n=$((tf_run_n + 1)); fi
    if [ "$g2_comp" -ge 0 ]; then g2_comp_sum=$((g2_comp_sum + g2_comp)); g2_comp_n=$((g2_comp_n + 1)); fi
    if [ "$g2_run" -ge 0 ]; then g2_run_sum=$((g2_run_sum + g2_run)); g2_run_n=$((g2_run_n + 1)); fi

    # 9. Notes column
    notes="${NOTES[$name]:-}"

    # 10. Output markdown row
    printf "| %d | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |\n" \
        "$total" "$name" "$correct" \
        "$(fmt_ms $tf_comp)" \
        "$(fmt_ms $g0_comp)" "$(fmt_ms $g1_comp)" "$(fmt_ms $g2_comp)" "$(fmt_ms $g3_comp)" \
        "$(fmt_ms $tf_run)" \
        "$(fmt_ms $g0_run)" "$(fmt_ms $g1_run)" "$(fmt_ms $g2_run)" "$(fmt_ms $g3_run)" \
        "$(fmt_ratio $tf_run $g2_run)" \
        "$notes" >> "$OUTFILE"

    # Progress
    printf "."
done

echo ""
echo ""

# Summary
{
    echo ""
    echo "## Correctness Summary"
    echo ""
    echo "- **Correct (tf = gcc)**: $yes"
    echo "- **Wrong output (tf != gcc)**: $no"
    echo "- **sixth.fs no output**: $tffail"
    echo "- **C translation failed**: $nt"
    echo "- **GCC compile/run failed**: $cfail"
    echo "- **Both no output**: $both"
    echo "- **Total**: $total"
    echo ""
    echo "## Performance Summary"
    echo ""
    if [ "$tf_comp_n" -gt 0 ]; then
        echo "- sixth.fs avg compile: $(fmt_ms $((tf_comp_sum / tf_comp_n)))ms"
    fi
    if [ "$g2_comp_n" -gt 0 ]; then
        echo "- gcc-O2 avg compile: $(fmt_ms $((g2_comp_sum / g2_comp_n)))ms"
    fi
    if [ "$tf_run_n" -gt 0 ]; then
        echo "- sixth.fs avg runtime: $(fmt_ms $((tf_run_sum / tf_run_n)))ms"
    fi
    if [ "$g2_run_n" -gt 0 ]; then
        echo "- gcc-O2 avg runtime: $(fmt_ms $((g2_run_sum / g2_run_n)))ms"
    fi
} >> "$OUTFILE"

echo "Written to $OUTFILE"
echo "Correct: $yes / $total"
echo "Wrong: $no"
echo "TF-fail: $tffail"
echo "C-fail: $((nt + cfail + both))"
