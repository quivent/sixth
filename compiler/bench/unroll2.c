// expected: 1000000000
// Manually unrolled 2x, 500M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 500000000; i++) {
        sum++;
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
