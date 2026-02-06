// Sieve of Eratosthenes benchmark
#include <stdio.h>
#include <string.h>

#define N 100000

char sieve[N + 1];

int main() {
    memset(sieve, 1, sizeof(sieve));
    sieve[0] = sieve[1] = 0;

    for (int i = 2; i * i <= N; i++) {
        if (sieve[i]) {
            for (int j = i * i; j <= N; j += i) {
                sieve[j] = 0;
            }
        }
    }

    int count = 0;
    for (int i = 2; i <= N; i++) {
        if (sieve[i]) count++;
    }

    printf("%d\n", count);
    return 0;
}
