// expected: 0
// CRC32 checksum - compute CRC32 over many iterations
#include <stdio.h>
#include <stdint.h>

uint32_t crc_table[256];
unsigned char test_data[256];

void init_table(void) {
    for (int i = 0; i < 256; i++) {
        uint32_t crc = i;
        for (int j = 0; j < 8; j++) {
            if (crc & 1)
                crc = (crc >> 1) ^ 0xEDB88320;
            else
                crc >>= 1;
        }
        crc_table[i] = crc;
    }
}

uint32_t crc32(unsigned char *data, int len) {
    uint32_t crc = 0xFFFFFFFF;
    for (int i = 0; i < len; i++) {
        crc = crc_table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
    }
    return crc ^ 0xFFFFFFFF;
}

int main(void) {
    init_table();
    for (int i = 0; i < 256; i++) test_data[i] = i;
    uint32_t result = 0;
    for (int i = 0; i < 10000; i++) {
        result ^= crc32(test_data, 256);
    }
    printf("%u\n", result);
    return 0;
}
