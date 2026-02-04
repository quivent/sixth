// expected: 50008
// Kolakoski sequence - self-describing sequence of 1s and 2s
#include <stdio.h>

#define MAXLEN 100000

char seq[MAXLEN];

void gen_kolakoski(int n) {
    seq[0] = 1;
    seq[1] = 2;
    seq[2] = 2;
    int len = 3;
    int i = 1;
    int curval = 2;
    while (len < n) {
        int runlen = seq[i];
        for (int j = 0; j < runlen && len < n; j++) {
            seq[len++] = curval;
        }
        i++;
        curval = (curval == 1) ? 2 : 1;
    }
}

int count_ones(int n) {
    int count = 0;
    for (int i = 0; i < n; i++) {
        if (seq[i] == 1) count++;
    }
    return count;
}

int main(void) {
    gen_kolakoski(MAXLEN);
    printf("%d\n", count_ones(MAXLEN));
    return 0;
}
