// expected: 233168
// Sum multiples of 3 and 5 benchmark - sum below 1000, run 100000 times

#include <stdio.h>

long sum_multiples(int limit) {
    long sum = 0;
    for (int i = 1; i < limit; i++) {
        if (i % 3 == 0 || i % 5 == 0) {
            sum += i;
        }
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 100000; i++) {
        result = sum_multiples(1000);
    }
    printf("%ld\n", result);
    return 0;
}
