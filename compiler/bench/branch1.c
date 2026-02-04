// expected: 50000000
// Predictable branch pattern - CPU can predict
#include <stdio.h>

static int classify(long n) { return n % 2; }

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += classify(i);
    }
    printf("%ld\n", sum);
    return 0;
}
