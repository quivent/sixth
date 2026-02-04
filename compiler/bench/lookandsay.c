// expected: 507345
// Look-and-say sequence - describe previous term (15 iterations)
#include <stdio.h>

#define MAXLEN 50000

char buf1[MAXLEN], buf2[MAXLEN];
char *src, *dst;
int slen, dlen;

void init_seq(void) {
    src = buf1; dst = buf2;
    buf1[0] = 1;
    slen = 1;
}

void swap_bufs(void) {
    char *t = src; src = dst; dst = t;
    slen = dlen;
    dlen = 0;
}

void next_seq(void) {
    dlen = 0;
    int i = 0;
    while (i < slen) {
        char ch = src[i];
        int count = 1;
        while (i + count < slen && src[i + count] == ch) {
            count++;
        }
        dst[dlen++] = count + '0';
        dst[dlen++] = ch + '0';
        i += count;
    }
    swap_bufs();
}

int seq_sum(void) {
    int sum = 0;
    for (int i = 0; i < slen; i++) {
        sum += src[i];
    }
    return sum;
}

int main(void) {
    init_seq();
    for (int i = 0; i < 15; i++) {
        next_seq();
    }
    printf("%d\n", seq_sum());
    return 0;
}
