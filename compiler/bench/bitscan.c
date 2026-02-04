// expected: 9999985
// Bit scan forward - find lowest set bit position
#include <stdio.h>
#include <stdint.h>

int bsf(uint32_t n) {
    if (n == 0) return -1;
    int pos = 0;
    while ((n & 1) == 0) {
        n >>= 1;
        pos++;
    }
    return pos;
}

int main(void) {
    long sum = 0;
    for (int i = 1; i < 10000000; i++) {
        sum += bsf(i);
    }
    printf("%ld\n", sum);
    return 0;
}
