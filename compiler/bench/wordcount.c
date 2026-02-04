// expected: 12
// Word count benchmark - count words in text, 100000 times

#include <stdio.h>
#include <string.h>

char text[128];

void init_text(void) {
    strcpy(text, "The quick brown fox jumps over the lazy dog near the river");
}

int is_space(char c) {
    return c == ' ' || c == '\t';
}

int count_words(const char *s, int len) {
    int count = 0;
    int in_word = 0;
    for (int i = 0; i < len; i++) {
        if (is_space(s[i])) {
            if (in_word) {
                in_word = 0;
            }
        } else {
            if (!in_word) {
                count++;
                in_word = 1;
            }
        }
    }
    return count;
}

int main(void) {
    init_text();
    int result = 0;
    for (int i = 0; i < 100000; i++) {
        result = count_words(text, 60);
    }
    printf("%d\n", result);
    return 0;
}
