// expected: 5000000
// Gray code - convert binary to Gray code and back
#include <stdio.h>
#include <stdint.h>

uint32_t to_gray(uint32_t n) {
    return n ^ (n >> 1);
}

uint32_t from_gray(uint32_t gray) {
    uint32_t n = gray;
    while (gray >>= 1) {
        n ^= gray;
    }
    return n;
}

int main(void) {
    long count = 0;
    for (int i = 0; i < 5000000; i++) {
        if (from_gray(to_gray(i)) == i) count++;
    }
    printf("%ld\n", count);
    return 0;
}
