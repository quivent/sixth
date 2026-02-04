// expected: 300000000
// Live range splitting - values used at different points
#include <stdio.h>

static long process(long c) {
    return c * c + 2;
}

int main() {
    long sum = 0;
    for (long i = 0; i < 100000000; i++) {
        sum += process(1);
    }
    printf("%ld\n", sum);
    return 0;
}
