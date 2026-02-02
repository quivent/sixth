// expected: 1637505
#include <stdio.h>
long collatz_len(long n) {
    long steps = 0;
    while (n > 1) {
        if (n % 2 == 0) n /= 2;
        else n = 3 * n + 1;
        steps++;
    }
    return steps;
}
int main() {
    long sum = 0;
    for (long i = 1; i < 100001; i++)
        sum += collatz_len(i);
    printf("%ld\n", sum);
    return 0;
}
