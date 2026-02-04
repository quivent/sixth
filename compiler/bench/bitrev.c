// expected: 0
// Bit reverse - reverse bits in a 32-bit number
#include <stdio.h>
#include <stdint.h>

uint32_t bitrev32(uint32_t n) {
    uint32_t result = 0;
    for (int i = 0; i < 32; i++) {
        result = (result << 1) | (n & 1);
        n >>= 1;
    }
    return result;
}

int main(void) {
    uint32_t result = 0;
    for (int i = 0; i < 5000000; i++) {
        result ^= bitrev32(i);
    }
    printf("%u\n", result);
    return 0;
}
