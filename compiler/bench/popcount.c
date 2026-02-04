// expected: 114434624
// Population count - count bits set in numbers
#include <stdio.h>
#include <stdint.h>

int popcount(uint32_t n) {
    int count = 0;
    while (n) {
        count += n & 1;
        n >>= 1;
    }
    return count;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 10000000; i++) {
        sum += popcount(i);
    }
    printf("%ld\n", sum);
    return 0;
}
