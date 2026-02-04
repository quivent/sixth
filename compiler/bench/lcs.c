// expected: 60000
// Longest common subsequence, 10K pairs of strings (length 8)
// Checksum: sum of all LCS lengths

#include <stdio.h>

char s1[16];
char s2[16];
int dp[9][9];

int max2(int a, int b) { return a > b ? a : b; }

void fill_s1(int n) {
    for (int i = 0; i < 8; i++) {
        s1[i] = 'a' + ((i + n) % 26);
    }
}

void fill_s2(int n) {
    for (int i = 0; i < 8; i++) {
        s2[i] = 'a' + ((i + n + 2) % 26);
    }
}

int lcs_len(void) {
    for (int i = 0; i <= 8; i++) {
        dp[i][0] = 0;
        dp[0][i] = 0;
    }
    for (int j = 0; j < 8; j++) {
        for (int i = 0; i < 8; i++) {
            if (s1[j] == s2[i]) {
                dp[j+1][i+1] = dp[j][i] + 1;
            } else {
                dp[j+1][i+1] = max2(dp[j+1][i], dp[j][i+1]);
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
        sum += lcs_len();
    }
    printf("%ld\n", sum);
    return 0;
}
