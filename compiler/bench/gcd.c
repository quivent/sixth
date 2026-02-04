// expected: 3619
// GCD benchmark - compute GCD of various pairs, 500000 times

#include <stdio.h>

int gcd(int a, int b) {
    while (b) {
        int t = b;
        b = a % b;
        a = t;
    }
    return a;
}

int bench_gcd(void) {
    int sum = 0;
    sum += gcd(1071, 1029);
    sum += gcd(3528, 3780);
    sum += gcd(9999, 3333);
    sum += gcd(1234567, 7654321);
    sum += gcd(48, 18);
    sum += gcd(100, 35);
    sum += gcd(17, 13);
    return sum;
}

int main(void) {
    int result = 0;
    for (int i = 0; i < 500000; i++) {
        result = bench_gcd();
    }
    printf("%d\n", result);
    return 0;
}
