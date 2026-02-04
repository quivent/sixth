// expected: 156566624
// Hamming distance - count differing bits between two numbers
#include <stdio.h>
#include <stdint.h>

int hamming(uint32_t a, uint32_t b) {
    uint32_t x = a ^ b;
    int count = 0;
    while (x) {
        count += x & 1;
        x >>= 1;
    }
    return count;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 5000; i++) {
        for (int j = 0; j < 5000; j++) {
            sum += hamming(i, j);
        }
    }
    printf("%ld\n", sum);
    return 0;
}
