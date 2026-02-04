// expected: 0
// Find substring 100K times
// Checksum: sum of found positions

#include <stdio.h>

char haystack[64];
char needle[8];

void fill_hay(int n) {
    for (int i = 0; i < 32; i++) {
        haystack[i] = 'A' + ((i + n) % 26);
    }
}

void fill_needle(int n) {
    for (int i = 0; i < 4; i++) {
        needle[i] = 'A' + (n % 26);
    }
}

int match_at(int pos) {
    for (int i = 0; i < 4; i++) {
        if (haystack[pos + i] != needle[i]) return 0;
    }
    return 1;
}

int find_sub(void) {
    for (int i = 0; i < 29; i++) {
        if (match_at(i)) return i;
    }
    return -1;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        fill_hay(i);
        fill_needle(i);
        int pos = find_sub();
        if (pos >= 0) sum += pos;
    }
    printf("%ld\n", sum);
    return 0;
}
