// expected: 262
// Hailstone/Collatz benchmark - find longest sequence under 10000, run 100 times

#include <stdio.h>

int hailstone_len(long n) {
    int len = 1;
    while (n > 1) {
        if (n & 1) {
            n = 3 * n + 1;
        } else {
            n /= 2;
        }
        len++;
    }
    return len;
}

int max_hailstone(int limit) {
    int maxlen = 0;
    for (int i = 1; i < limit; i++) {
        int len = hailstone_len(i);
        if (len > maxlen) maxlen = len;
    }
    return maxlen;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 100; i++) {
        result = max_hailstone(10000);
    }
    printf("%d\n", result);
    return 0;
}
