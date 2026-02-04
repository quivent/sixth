// expected: 30393485
// Euler's totient function - count coprime numbers
#include <stdio.h>

int gcd(int a, int b) {
    while (b) {
        int t = b;
        b = a % b;
        a = t;
    }
    return a;
}

int totient(int n) {
    int count = 0;
    for (int i = 1; i < n; i++) {
        if (gcd(n, i) == 1) count++;
    }
    return count;
}

int main(void) {
    long sum = 0;
    for (int i = 1; i < 10000; i++) {
        sum += totient(i);
    }
    printf("%ld\n", sum);
    return 0;
}
