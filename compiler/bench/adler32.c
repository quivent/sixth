// expected: 0
// Adler32 checksum - compute adler32 over many iterations
#include <stdio.h>
#include <stdint.h>

#define MOD_ADLER 65521

unsigned char test_data[256];

uint32_t adler32(unsigned char *data, int len) {
    uint32_t a = 1, b = 0;
    for (int i = 0; i < len; i++) {
        a = (a + data[i]) % MOD_ADLER;
        b = (b + a) % MOD_ADLER;
    }
    return (b << 16) | a;
}

int main(void) {
    for (int i = 0; i < 256; i++) test_data[i] = i;
    uint32_t result = 0;
    for (int i = 0; i < 50000; i++) {
        result ^= adler32(test_data, 256);
    }
    printf("%u\n", result);
    return 0;
}
