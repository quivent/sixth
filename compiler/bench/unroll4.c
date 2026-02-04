// expected: 1000000000
// Manually unrolled 4x, 250M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 250000000; i++) {
        sum++;
        sum++;
        sum++;
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
