// expected: 77499560
// Concatenate 100K times
// Checksum: sum of final string first char value * length

#include <stdio.h>

char buf[256];
int len;

void reset_buf(void) { len = 0; }

void append_char(char c) {
    buf[len++] = c;
}

void append_str(int n) {
    char c = 'A' + (n % 26);
    for (int i = 0; i < 10; i++) {
        append_char(c);
    }
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 100000; i++) {
        reset_buf();
        append_str(i);
        sum += buf[0] * len;
    }
    printf("%ld\n", sum);
    return 0;
}
