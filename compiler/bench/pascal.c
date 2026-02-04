// expected: 6552127682118890048
#include <stdio.h>
#define SIZE 501
long memo[SIZE][SIZE];
int computed[SIZE][SIZE];

long pascal(long n, long k) {
    if (k == 0 || k == n) return 1;
    if (computed[n][k]) return memo[n][k];
    memo[n][k] = pascal(n - 1, k - 1) + pascal(n - 1, k);
    computed[n][k] = 1;
    return memo[n][k];
}

int main() {
    for (int i = 0; i < SIZE; i++)
        for (int j = 0; j < SIZE; j++)
            computed[i][j] = 0;
    printf("%ld\n", pascal(500, 250));
    return 0;
}
