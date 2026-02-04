// expected: -2872092127636481573
#include <stdio.h>
#define MEMO_SIZE 10001
long memo[MEMO_SIZE];

void fibmemo_init() {
    for (int i = 0; i < MEMO_SIZE; i++) memo[i] = 0;
    memo[1] = 1;
}

long fibmemo(long n) {
    if (n < 2) return n;
    for (long i = 2; i <= n; i++) {
        memo[i] = memo[i-1] + memo[i-2];
    }
    return memo[n];
}

int main() {
    fibmemo_init();
    printf("%ld\n", fibmemo(10000));
    return 0;
}
