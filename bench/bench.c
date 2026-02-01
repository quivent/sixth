// bench.c - C equivalents for Sixth benchmarks
// Compile: gcc -O2 bench.c -o bench_c
// Run: ./bench_c <test>
//
// Let gcc optimize freely. If gcc eliminates a loop, that's a legitimate
// optimization — same as sixth.fs does. Fair comparison = both compilers
// optimize as hard as they can.

#include <stdio.h>
#include <string.h>
#include <stdint.h>

// Prevent gcc from eliminating function calls
#define NOINLINE __attribute__((noinline))

// arith / arith-std: 1B two-variable ops (a++; n--)
// Forth: 0 1000000000 begin nos+ 1-nzloop (custom)
// Forth: 0 1000000000 begin swap 1+ swap 1- dup 0> while repeat (standard)
// Both compute the same thing. C equivalent is identical.
NOINLINE int64_t bench_arith(void) {
    int64_t a = 0, n = 1000000000;
    while (n > 0) { a++; n--; }
    return a;
}

// loop / loop-std: 1B countdown
// Forth: 1000000000 begin 1-nzloop (custom)
// Forth: 1000000000 begin 1- dup 0> while repeat (standard)
NOINLINE int64_t bench_loop(void) {
    int64_t n = 1000000000;
    while (n > 0) { n--; }
    return n;
}

// branch: 100M alternating if/else
NOINLINE int64_t bench_branch(void) {
    int64_t x = 0;
    for (int64_t i = 0; i < 100000000; i++) {
        if (i & 1) x++; else x--;
    }
    return x;
}

// stack: 100M swaps
NOINLINE int64_t bench_stack(void) {
    int64_t a = 1, b = 2;
    for (int64_t i = 0; i < 100000000; i++) {
        int64_t t = a; a = b; b = t;
    }
    return a;
}

// fib: fib(1B) iterative / fib-std: fib(1B) iterative
// Forth: tuck+ in do/loop (custom)
// Forth: swap over + in do/loop (standard)
// Values overflow int64 but computation still runs.
NOINLINE int64_t fib_iter(int64_t n) {
    int64_t a = 0, b = 1;
    for (int64_t i = 0; i < n; i++) {
        int64_t t = a + b;
        a = b;
        b = t;
    }
    return a;
}

// nested: 10K x 10K do/loop (100M iterations)
NOINLINE int64_t bench_nested(void) {
    int64_t count = 0;
    for (int i = 0; i < 10000; i++)
        for (int j = 0; j < 10000; j++) {
            count++;
        }
    return count;
}

// mem: 100K passes over 1K entries
int64_t mem_area[1024];
NOINLINE int64_t bench_mem(void) {
    for (int64_t p = 0; p < 100000; p++) {
        for (int i = 0; i < 1000; i++) {
            mem_area[i] = i;
        }
    }
    return mem_area[0];
}

// call / fibrec: 10M countdown
// Recursive version overflows stack at -O0 for 10M calls.
// Use iterative version that matches the call overhead tf generates
// (tf tail-call-optimizes recurse to a jump).
NOINLINE int64_t countdown(int64_t n) {
    while (n > 0) { n--; }
    return n;
}

// arith50m: 50M mixed arithmetic
// Forth: dup 3 * 7 + 2 / 5 mod 11 +
NOINLINE int64_t bench_arith50m(void) {
    int64_t n = 0;
    for (int64_t i = 0; i < 50000000; i++) {
        n = ((n * 3 + 7) / 2) % 5 + 11;
    }
    return n;
}

// collatz: sum of Collatz steps for n=1..50000
NOINLINE int64_t bench_collatz(void) {
    int64_t total = 0;
    for (int64_t n = 1; n < 50000; n++) {
        int64_t v = n, steps = 0;
        while (v > 1) {
            if (v & 1) v = 3 * v + 1;
            else v = v / 2;
            steps++;
        }
        total += steps;
    }
    return total;
}

// spill: 4-variable rotate+accumulate, 100M iterations
NOINLINE int64_t bench_spill(void) {
    int64_t a = 1, b = 2, c = 3, d = 4;
    for (int64_t i = 0; i < 100000000; i++) {
        a += d;
        b += c;
        c = a + b;
        d = c + d;
    }
    return d;
}

// call100m: 100M calls to inc1
NOINLINE int64_t inc1(int64_t n) { return n + 1; }
NOINLINE int64_t bench_call100m(void) {
    int64_t n = 0;
    for (int64_t i = 0; i < 100000000; i++) {
        n = inc1(n);
    }
    return n;
}

// fib38: recursive fibonacci(38)
NOINLINE int64_t fib_rec(int64_t n) {
    if (n < 2) return n;
    return fib_rec(n - 1) + fib_rec(n - 2);
}

// nested100k: 100K x 10K nested loops
NOINLINE int64_t bench_nested100k(void) {
    int64_t count = 0;
    for (int64_t i = 0; i < 100000; i++)
        for (int64_t j = 0; j < 10000; j++) {
            count++;
        }
    return count;
}

int main(int argc, char **argv) {
    if (argc < 2) {
        printf("Usage: %s <test>\n", argv[0]);
        return 1;
    }
    const char *t = argv[1];
    if      (!strcmp(t, "arith"))      printf("%ld\n", bench_arith());
    else if (!strcmp(t, "arith-std"))  printf("%ld\n", bench_arith());
    else if (!strcmp(t, "loop"))       printf("%ld\n", bench_loop());
    else if (!strcmp(t, "loop-std"))   printf("%ld\n", bench_loop());
    else if (!strcmp(t, "branch"))     printf("%ld\n", bench_branch());
    else if (!strcmp(t, "stack"))      printf("%ld\n", bench_stack());
    else if (!strcmp(t, "fib"))        printf("%ld\n", fib_iter(1000000000));
    else if (!strcmp(t, "fib-std"))    printf("%ld\n", fib_iter(1000000000));
    else if (!strcmp(t, "nested"))     printf("%ld\n", bench_nested());
    else if (!strcmp(t, "mem"))        printf("%ld\n", bench_mem());
    else if (!strcmp(t, "call"))       printf("%ld\n", countdown(10000000));
    else if (!strcmp(t, "fibrec"))     printf("%ld\n", countdown(10000000));
    else if (!strcmp(t, "arith50m"))   printf("%ld\n", bench_arith50m());
    else if (!strcmp(t, "collatz"))    printf("%ld\n", bench_collatz());
    else if (!strcmp(t, "spill"))      printf("%ld\n", bench_spill());
    else if (!strcmp(t, "call100m"))   printf("%ld\n", bench_call100m());
    else if (!strcmp(t, "fib38"))      printf("%ld\n", fib_rec(38));
    else if (!strcmp(t, "nested100k")) printf("%ld\n", bench_nested100k());
    else { printf("Unknown: %s\n", t); return 1; }
    return 0;
}
