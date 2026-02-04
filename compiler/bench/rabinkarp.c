// expected: 0
// Rabin-Karp search, 100K searches
// Checksum: sum of found positions

#include <stdio.h>

char text[64];
char pattern[8];
int pat_len;
#define PRIME 101
#define BASE 256

void fill_text(int n) {
    for (int i = 0; i < 32; i++) {
        text[i] = 'a' + ((i + n) % 26);
    }
}

void fill_pat(int n) {
    // Pattern is 3 consecutive letters starting at offset n%26
    for (int i = 0; i < 3; i++) {
        pattern[i] = 'a' + ((n + i) % 26);
    }
    pat_len = 3;
}

int pat_hash(void) {
    int h = 0;
    for (int i = 0; i < pat_len; i++) {
        h = (h * BASE + pattern[i]) % PRIME;
    }
    return h;
}

int text_hash(int pos) {
    int h = 0;
    for (int i = 0; i < pat_len; i++) {
        h = (h * BASE + text[pos + i]) % PRIME;
    }
    return h;
}

int match_at(int pos) {
    for (int i = 0; i < pat_len; i++) {
        if (text[pos + i] != pattern[i]) return 0;
    }
    return 1;
}

int rk_search(void) {
    int ph = pat_hash();
    int th = text_hash(0);
    if (ph == th && match_at(0)) return 0;

    int h = 1;
    for (int i = 0; i < pat_len - 1; i++) h = (h * BASE) % PRIME;

    for (int i = 1; i <= 32 - pat_len; i++) {
        th = ((th - text[i-1] * h % PRIME + PRIME) * BASE + text[i + pat_len - 1]) % PRIME;
        if (th == ph && match_at(i)) return i;
    }
    return -1;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        fill_text(i);
        fill_pat(i);
        int pos = rk_search();
        if (pos >= 0) sum += pos;
    }
    printf("%ld\n", sum);
    return 0;
}
