// expected: 198
// Palindrome benchmark - count palindromic numbers 1-9999, run 200 times

#include <stdio.h>

char numbuf[12];

int num_to_str(int n, char *buf) {
    char *p = buf + 11;
    *p = '\0';
    do {
        *--p = '0' + (n % 10);
        n /= 10;
    } while (n);
    int len = buf + 11 - p;
    for (int i = 0; i < len; i++) {
        buf[i] = p[i];
    }
    return len;
}

int palindrome(char *s, int len) {
    char *end = s + len - 1;
    while (s < end) {
        if (*s != *end) return 0;
        s++;
        end--;
    }
    return 1;
}

int num_palindrome(int n) {
    int len = num_to_str(n, numbuf);
    return palindrome(numbuf, len);
}

int count_palindromes(int limit) {
    int count = 0;
    for (int i = 1; i < limit; i++) {
        if (num_palindrome(i)) count++;
    }
    return count;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 200; i++) {
        result = count_palindromes(10000);
    }
    printf("%d\n", result);
    return 0;
}
