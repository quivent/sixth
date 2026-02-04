// expected: 5
// Text wrap benchmark - wrap text at width, count lines, 50000 times

#include <stdio.h>
#include <string.h>

char text[128];

void init_text(void) {
    strcpy(text, "The quick brown fox jumps over the lazy dog near the river bank");
}

int wrap_count(const char *s, int len, int width) {
    int lines = 1;
    int col = 0;
    int last_space = 0;
    for (int i = 0; i < len; i++) {
        if (s[i] == ' ') {
            last_space = i;
        }
        if (col >= width) {
            lines++;
            col = 0;
        } else {
            col++;
        }
    }
    return lines;
}

int main(void) {
    init_text();
    int result = 0;
    for (int i = 0; i < 50000; i++) {
        result = wrap_count(text, 64, 12);
    }
    printf("%d\n", result);
    return 0;
}
