// expected: 1250000025000000
// Simple unrollable loop - GCC unrolls fixed-count loops
#include <stdio.h>

int main() {
    long sum = 0;
    for (long i = 0; i <= 50000000; i++) sum += i;
    printf("%ld\n", sum);
    return 0;
}
