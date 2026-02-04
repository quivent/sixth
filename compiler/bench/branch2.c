// expected: 2500000000000000
// Branchless vs branch - GCC can use cmov
#include <stdio.h>

static long absval(long n) { return n < 0 ? -n : n; }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += absval(i - 50000000);
    }
    printf("%ld\n", sum);
    return 0;
}
