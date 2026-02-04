// expected: 266332
// FizzBuzz benchmark - sum non-fizzbuzz numbers 1-1000, run 1000 times

#include <stdio.h>

long fizzbuzz_sum(int limit) {
    long sum = 0;
    for (int i = 1; i <= limit; i++) {
        if (i % 15 == 0) {
            // FizzBuzz
        } else if (i % 3 == 0) {
            // Fizz
        } else if (i % 5 == 0) {
            // Buzz
        } else {
            sum += i;
        }
    }
    return sum;
}

int main(void) {
    long result = 0;
    for (int i = 0; i < 1000; i++) {
        result = fizzbuzz_sum(1000);
    }
    printf("%ld\n", result);
    return 0;
}
