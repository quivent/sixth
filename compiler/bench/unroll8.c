// expected: 1000000000
// Manually unrolled 8x, 125M iterations
#include <stdio.h>

int main(void) {
    long sum = 0;
    for (long i = 0; i < 125000000; i++) {
        sum++;
        sum++;
        sum++;
        sum++;
        sum++;
        sum++;
        sum++;
        sum++;
    }
    printf("%ld\n", sum);
    return 0;
}
