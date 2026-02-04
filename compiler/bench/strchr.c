// expected: 4615411
// Find char 1M times
// Checksum: sum of found positions (0-indexed, -1 if not found)

#include <stdio.h>

char buf[32];

void fill_buf(int n) {
    (void)n;
    for (int i = 0; i < 16; i++) {
        buf[i] = 'A' + i;
    }
}

int find_char(char c) {
    for (int i = 0; i < 16; i++) {
        if (buf[i] == c) return i;
    }
    return -1;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 1000000; i++) {
        fill_buf(i);
        int pos = find_char('A' + (i % 26));
        if (pos >= 0) sum += pos;
    }
    printf("%ld\n", sum);
    return 0;
}
