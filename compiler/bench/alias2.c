// expected: 102400000
// Restrict-like pattern - read from one, write to another
#include <stdio.h>
#define SIZE 1024

long src[SIZE], dst[SIZE];

void copy_scaled(long * restrict d, const long * restrict s) {
    for (int i = 0; i < SIZE; i++) d[i] = s[i] * 2;
}

int main() {
    for (int i = 0; i < SIZE; i++) src[i] = i;
    long sum = 0;
    for (int j = 0; j < 100000; j++) {
        copy_scaled(dst, src);
        sum += SIZE;
    }
    printf("%ld\n", sum);
    return 0;
}
