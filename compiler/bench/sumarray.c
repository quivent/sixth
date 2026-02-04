// expected: 49995000
// Sum and product array benchmark - sum 10000 elements, run 1000 times

#include <stdio.h>

#define SIZE 10000

int arr[SIZE];

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = i;
    }
}

long sum_arr(void) {
    long sum = 0;
    for (int i = 0; i < SIZE; i++) {
        sum += arr[i];
    }
    return sum;
}

int main(void) {
    init_arr();
    long result = 0;
    for (int i = 0; i < 1000; i++) {
        result = sum_arr();
    }
    printf("%ld\n", result);
    return 0;
}
