// expected: 1500000000
// Constant propagation through function calls
#include <stdio.h>

static long add5(long n) { return n + 5; }
static long mul3(long n) { return n * 3; }
static long compute(long n) { return mul3(add5(n)); }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += compute(0);
    }
    printf("%ld\n", sum);
    return 0;
}
