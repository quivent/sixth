// expected: 60000
// Edit distance, 10K pairs of short strings (length 8)
// Checksum: sum of all distances

#include <stdio.h>

char s1[16];
char s2[16];
int dp[9][9];

int min2(int a, int b) { return a < b ? a : b; }
int min3(int a, int b, int c) { return min2(min2(a, b), c); }

void fill_s1(int n) {
    for (int i = 0; i < 8; i++) {
        s1[i] = 'a' + ((i + n) % 26);
    }
}

void fill_s2(int n) {
    for (int i = 0; i < 8; i++) {
        s2[i] = 'a' + ((i + n + 3) % 26);
    }
}

int levenshtein(void) {
    for (int i = 0; i <= 8; i++) {
        dp[i][0] = i;
        dp[0][i] = i;
    }
    for (int j = 0; j < 8; j++) {
        for (int i = 0; i < 8; i++) {
            if (s1[j] == s2[i]) {
                dp[j+1][i+1] = dp[j][i];
            } else {
                dp[j+1][i+1] = min3(dp[j][i] + 1, dp[j+1][i] + 1, dp[j][i+1] + 1);
            }
        }
    }
    return dp[8][8];
}

int main(void) {
    long sum = 0;
    for (int i = 0; i < 10000; i++) {
        fill_s1(i);
        fill_s2(i);
        sum += levenshtein();
    }
    printf("%ld\n", sum);
    return 0;
}
