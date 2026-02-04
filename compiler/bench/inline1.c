// expected: 300000000
// Tiny function called 100M times - GCC inlines completely
#include <stdio.h>

static long inc3(long n) { return n + 1 + 1 + 1; }

int main() {
    long x = 0;
    for (int i = 0; i < 100000000; i++) x = inc3(x);
    printf("%ld\n", x);
    return 0;
}
