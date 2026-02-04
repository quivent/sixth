// expected: 774920
// Reverse 10K strings of length 100
// Checksum: sum of first char after reverse (which was last char before)

#include <stdio.h>

char buf[128];

void fill_buf(int n) {
    for (int i = 0; i < 100; i++) {
        buf[i] = 'A' + (n % 26);
    }
}

void reverse_buf(void) {
    for (int i = 0; i < 50; i++) {
        char t = buf[i];
        buf[i] = buf[99 - i];
        buf[99 - i] = t;
    }
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 10000; i++) {
        fill_buf(i);
        reverse_buf();
        sum += buf[0];
    }
    printf("%ld\n", sum);
    return 0;
}
