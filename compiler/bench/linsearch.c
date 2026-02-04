// expected: 49950000
// Linear search benchmark - search 100K times in unsorted array

#include <stdio.h>

#define SIZE 1000
#define ITERS 100000

int arr[SIZE];

int lsearch(int val) {
    for (int i = 0; i < SIZE; i++) {
        if (arr[i] == val) return i;
    }
    return -1;
}

void init_arr(void) {
    // Shuffle pattern: store (i*7) mod SIZE at index i
    for (int i = 0; i < SIZE; i++) {
        arr[i] = (i * 7) % SIZE;
    }
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        sum += lsearch(i % SIZE);
    }
    return sum;
}

int main(void) {
    init_arr();
    printf("%ld\n", bench());
    return 0;
}
