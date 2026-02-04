// expected: 499500
// Binary search benchmark - search sorted array, 100 iterations

#include <stdio.h>

#define SIZE 1000

int arr[SIZE];

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = i;
    }
}

int binary_search(int val) {
    int lo = 0, hi = SIZE - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (arr[mid] == val) {
            return mid;
        } else if (arr[mid] < val) {
            lo = mid + 1;
        } else {
            hi = mid - 1;
        }
    }
    return -1;
}

long bench_search(void) {
    long sum = 0;
    for (int i = 0; i < SIZE; i++) {
        sum += binary_search(i);
    }
    return sum;
}

int main(void) {
    init_arr();
    long result = 0;
    for (int i = 0; i < 100; i++) {
        result = bench_search();
    }
    printf("%ld\n", result);
    return 0;
}
