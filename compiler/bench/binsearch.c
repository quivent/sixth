// expected: 499500000
// Binary search benchmark - search 1M times in sorted array

#include <stdio.h>

#define SIZE 1000
#define ITERS 1000000

int arr[SIZE];

int bsearch_impl(int val) {
    int lo = 0, hi = SIZE - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (arr[mid] == val) return mid;
        if (arr[mid] < val) {
            lo = mid + 1;
        } else {
            hi = mid - 1;
        }
    }
    return -1;
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = i;
    }
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        sum += bsearch_impl(i % SIZE);
    }
    return sum;
}

int main(void) {
    init_arr();
    printf("%ld\n", bench());
    return 0;
}
