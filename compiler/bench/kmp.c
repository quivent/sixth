// expected: 0
// KMP search, 100K searches
// Checksum: sum of found positions

#include <stdio.h>

char text[64];
char pattern[8];
int lps[8];
int pat_len;

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

void compute_lps(void) {
    lps[0] = 0;
    int len = 0;
    int i = 1;
    while (i < pat_len) {
        if (pattern[i] == pattern[len]) {
            len++;
            lps[i] = len;
            i++;
        } else {
            if (len > 0) {
                len = lps[len - 1];
            } else {
                lps[i] = 0;
                i++;
            }
        }
    }
}

int kmp_search(void) {
    compute_lps();
    int i = 0, j = 0;
    while (i < 32) {
        if (text[i] == pattern[j]) {
            i++;
            j++;
            if (j == pat_len) {
                return i - pat_len;
            }
        } else {
            if (j > 0) {
                j = lps[j - 1];
            } else {
                i++;
            }
        }
    }
    return -1;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        fill_text(i);
        fill_pat(i);
        int pos = kmp_search();
        if (pos >= 0) sum += pos;
    }
    printf("%ld\n", sum);
    return 0;
}
