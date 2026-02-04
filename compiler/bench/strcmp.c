// expected: 1000000
// Compare 1M string pairs
// Checksum: count of equal comparisons

#include <stdio.h>

char buf1[32];
char buf2[32];

void fill_str(char *buf, int n) {
    for (int i = 0; i < 8; i++) {
        buf[i] = (char)n;
    }
}

int cmp_str(void) {
    for (int i = 0; i < 8; i++) {
        if (buf1[i] != buf2[i]) return 0;
    }
    return 1;
}

int main(void) {
    long count = 0;
    for (long i = 0; i < 1000000; i++) {
        fill_str(buf1, i / 1000);
        fill_str(buf2, i / 1000);
        count += cmp_str();
    }
    printf("%ld\n", count);
    return 0;
}
