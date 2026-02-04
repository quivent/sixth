// expected: 12
// Roman numerals benchmark - max len of roman 1-1000, run 500 times
// Max is 888 = DCCCLXXXVIII = 12 chars

#include <stdio.h>

int roman_len(int n) {
    int len = 0;
    while (n >= 1000) { n -= 1000; len++; }
    if (n >= 900) { n -= 900; len += 2; }
    if (n >= 500) { n -= 500; len++; }
    if (n >= 400) { n -= 400; len += 2; }
    while (n >= 100) { n -= 100; len++; }
    if (n >= 90) { n -= 90; len += 2; }
    if (n >= 50) { n -= 50; len++; }
    if (n >= 40) { n -= 40; len += 2; }
    while (n >= 10) { n -= 10; len++; }
    if (n >= 9) { n -= 9; len += 2; }
    if (n >= 5) { n -= 5; len++; }
    if (n >= 4) { n -= 4; len += 2; }
    len += n;
    return len;
}

int bench_roman(int limit) {
    int maxlen = 0;
    for (int i = 1; i <= limit; i++) {
        int len = roman_len(i);
        if (len > maxlen) maxlen = len;
    }
    return maxlen;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 500; i++) {
        result = bench_roman(1000);
    }
    printf("%d\n", result);
    return 0;
}
