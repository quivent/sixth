// expected: 3734
// Caesar cipher benchmark - encode string, sum ASCII values, 50000 times

#include <stdio.h>
#include <string.h>

char teststr[64];
char outstr[64];

void init_str(void) {
    strcpy(teststr, "The quick brown fox jumps over lazy dog");
}

char caesar_char(char c, int shift) {
    if (c >= 'A' && c <= 'Z') {
        return 'A' + (c - 'A' + shift) % 26;
    }
    if (c >= 'a' && c <= 'z') {
        return 'a' + (c - 'a' + shift) % 26;
    }
    return c;
}

void caesar(const char *src, int len, int shift) {
    for (int i = 0; i < len; i++) {
        outstr[i] = caesar_char(src[i], shift);
    }
}

int sum_out(int len) {
    int sum = 0;
    for (int i = 0; i < len; i++) {
        sum += (unsigned char)outstr[i];
    }
    return sum;
}

int main(void) {
    init_str();
    int result = 0;
    for (int i = 0; i < 50000; i++) {
        caesar(teststr, 39, 5);
        result = sum_out(39);
    }
    printf("%d\n", result);
    return 0;
}
