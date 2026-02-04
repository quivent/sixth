// expected: 25500000
// Length of 1M strings
// Checksum: sum of all lengths

#include <stdio.h>

char buf[64];

void fill_buf(int n) {
    int len = (n % 50) + 1;
    for (int i = 0; i < len; i++) {
        buf[i] = 'A';
    }
    buf[len] = '\0';
}

int my_strlen(void) {
    int len = 0;
    while (buf[len]) len++;
    return len;
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 1000000; i++) {
        fill_buf(i);
        sum += my_strlen();
    }
    printf("%ld\n", sum);
    return 0;
}
