// expected: 499500000
// Interpolation search benchmark - search 1M times in sorted array

#include <stdio.h>

#define SIZE 1000
#define ITERS 1000000

int arr[SIZE];

int interp_search(int val) {
    int lo = 0, hi = SIZE - 1;
    while (lo <= hi && val >= arr[lo] && val <= arr[hi]) {
        if (arr[hi] == arr[lo]) {
            if (arr[lo] == val) return lo;
            return -1;
        }
        // pos = lo + ((val - arr[lo]) * (hi - lo)) / (arr[hi] - arr[lo])
        int pos = lo + ((long)(val - arr[lo]) * (hi - lo)) / (arr[hi] - arr[lo]);
        if (arr[pos] == val) return pos;
        if (arr[pos] < val) {
            hi = pos - 1;
        } else {
            lo = pos + 1;
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
        sum += interp_search(i % SIZE);
    }
    return sum;
}

int main(void) {
    init_arr();
    printf("%ld\n", bench());
    return 0;
}
