// expected: 32640
// Byte sum checksum - compute sum of bytes over many iterations
#include <stdio.h>
#include <stdint.h>

unsigned char test_data[256];

uint32_t bytesum(unsigned char *data, int len) {
    uint32_t result = 0;
    for (int i = 0; i < len; i++) {
        result += data[i];
    }
    return result;
}

int main(void) {
    for (int i = 0; i < 256; i++) test_data[i] = i;
    uint32_t result = 0;
    for (int i = 0; i < 500000; i++) {
        result ^= bytesum(test_data, 256);
    }
    printf("%u\n", result);
    return 0;
}
