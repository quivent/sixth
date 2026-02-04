// expected: 3000000000
// Constant folding across functions - nested constant expressions
#include <stdio.h>

static long k1() { return 7 + 3; }
static long k2() { return k1() * 2; }
static long k3() { return k2() + k1(); }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) sum += k3();
    printf("%ld\n", sum);
    return 0;
}
