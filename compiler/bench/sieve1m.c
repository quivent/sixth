// expected: 1229
#include <stdio.h>
#define LIMIT 10001
char sieve[LIMIT];
int main() {
    for (int i = 0; i < LIMIT; i++) sieve[i] = 1;
    sieve[0] = sieve[1] = 0;
    for (int i = 2; i * i < LIMIT; i++)
        if (sieve[i])
            for (int j = i * i; j < LIMIT; j += i)
                sieve[j] = 0;
    long count = 0;
    for (int i = 2; i < LIMIT; i++)
        if (sieve[i]) count++;
    printf("%ld\n", count);
    return 0;
}
