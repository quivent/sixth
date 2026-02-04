// expected: 0
// Fletcher16 checksum - compute fletcher16 over many iterations
#include <stdio.h>
#include <stdint.h>

unsigned char test_data[256];

uint32_t fletcher16(unsigned char *data, int len) {
    uint32_t sum1 = 0, sum2 = 0;
    for (int i = 0; i < len; i++) {
        sum1 = (sum1 + data[i]) % 255;
        sum2 = (sum2 + sum1) % 255;
    }
    return (sum2 << 8) | sum1;
}

int main(void) {
    for (int i = 0; i < 256; i++) test_data[i] = i;
    uint32_t result = 0;
    for (int i = 0; i < 100000; i++) {
        result ^= fletcher16(test_data, 256);
    }
    printf("%u\n", result);
    return 0;
}
