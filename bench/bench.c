// bench.c - C equivalents for Fifth benchmarks
// Compile: gcc -O2 bench.c -o bench_c
// Run: ./bench_c <test>

#include <stdio.h>
#include <string.h>
#include <stdint.h>

// arith: 10M two-variable ops
volatile int64_t arith_result;
void bench_arith() {
    int64_t a = 0, n = 10000000;
    while (n > 0) { a++; n--; }
    arith_result = a;
}

// branch: 10M alternating if/else
volatile int64_t branch_result;
void bench_branch() {
    int64_t x = 0;
    for (int i = 0; i < 10000000; i++) {
        if (i & 1) x++; else x--;
    }
    branch_result = x;
}

// call: 100K recursive calls
int64_t countdown(int64_t n) {
    if (n == 0) return 0;
    return countdown(n - 1);
}
void bench_call() { printf("%ld\n", countdown(100000)); }

// fib: fib(35) iterative
int64_t fib(int n) {
    int64_t a = 0, b = 1;
    for (int i = 0; i < n; i++) {
        int64_t t = a + b;
        a = b;
        b = t;
    }
    return a;
}
void bench_fib() { printf("%ld\n", fib(35)); }

// fibrec: 100K recursive countdown
void bench_fibrec() { printf("%ld\n", countdown(100000)); }

// loop: 100M decrement
volatile int64_t loop_result;
void bench_loop() {
    int64_t n = 100000000;
    while (n > 0) n--;
    loop_result = n;
}

// mem: 1K memory write/read
int64_t mem_area[1024];
void bench_mem() {
    for (int i = 0; i < 1000; i++) {
        mem_area[i] = i;
    }
    printf("%ld\n", mem_area[0]);
}

// nested: 1M nested loops
volatile int64_t nested_result;
void bench_nested() {
    int64_t count = 0;
    for (int i = 0; i < 1000; i++)
        for (int j = 0; j < 1000; j++)
            count++;
    nested_result = count;
}

// stack: 10M swaps
volatile int64_t stack_result;
void bench_stack() {
    int64_t a = 1, b = 2;
    for (int i = 0; i < 10000000; i++) {
        int64_t t = a; a = b; b = t;
    }
    stack_result = a;
}

int main(int argc, char **argv) {
    if (argc < 2) {
        printf("Usage: %s <test>\n", argv[0]);
        return 1;
    }
    if (!strcmp(argv[1], "arith"))  { bench_arith();  printf("%ld\n", arith_result); }
    else if (!strcmp(argv[1], "branch")) { bench_branch(); printf("%ld\n", branch_result); }
    else if (!strcmp(argv[1], "call"))   { bench_call(); }
    else if (!strcmp(argv[1], "fib"))    { bench_fib(); }
    else if (!strcmp(argv[1], "fibrec")) { bench_fibrec(); }
    else if (!strcmp(argv[1], "loop"))   { bench_loop();   printf("%ld\n", loop_result); }
    else if (!strcmp(argv[1], "mem"))    { bench_mem(); }
    else if (!strcmp(argv[1], "nested")) { bench_nested(); printf("%ld\n", nested_result); }
    else if (!strcmp(argv[1], "stack"))  { bench_stack();  printf("%ld\n", stack_result); }
    else { printf("Unknown test: %s\n", argv[1]); return 1; }
    return 0;
}
