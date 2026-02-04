// expected: 499500000
// Jump search benchmark - search 1M times in sorted array

#include <stdio.h>

#define SIZE 1000
#define ITERS 1000000
#define STEP 31

int arr[SIZE];

int min(int a, int b) { return a < b ? a : b; }

int jump_search(int val) {
    int prev = 0;
    int curr = STEP;
    while (curr < SIZE && arr[curr - 1] < val) {
        prev = curr;
        curr = min(curr + STEP, SIZE);
    }
    // Linear search from prev to curr
    for (int i = prev; i < min(curr, SIZE); i++) {
        if (arr[i] == val) return i;
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
        sum += jump_search(i % SIZE);
    }
    return sum;
}

int main(void) {
    init_arr();
    printf("%ld\n", bench());
    return 0;
}
