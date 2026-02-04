// expected: 700000000
// Chain of small functions - GCC inlines entire chain
#include <stdio.h>

static long f1(long n) { return n + 1; }
static long f2(long n) { return f1(n) + 1; }
static long f3(long n) { return f2(n) + 1; }
static long f4(long n) { return f3(n) + 1; }
static long f5(long n) { return f4(n) + 1; }
static long f6(long n) { return f5(n) + 1; }
static long f7(long n) { return f6(n) + 1; }

int main() {
    long x = 0;
    for (int i = 0; i < 100000000; i++) x = f7(x);
    printf("%ld\n", x);
    return 0;
}
