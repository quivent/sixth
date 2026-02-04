// expected: 19990000000
// Merge two sorted arrays benchmark - 10K iterations

#include <stdio.h>

#define SIZE 1000
#define ITERS 10000

int arr1[SIZE];
int arr2[SIZE];
int merged[SIZE * 2];

void merge_arrays(void) {
    int i = 0, j = 0, k = 0;
    while (i < SIZE || j < SIZE) {
        if (i >= SIZE) {
            merged[k++] = arr2[j++];
        } else if (j >= SIZE) {
            merged[k++] = arr1[i++];
        } else if (arr1[i] >= arr2[j]) {
            merged[k++] = arr1[i++];
        } else {
            merged[k++] = arr2[j++];
        }
    }
}

void init_arrays(void) {
    for (int i = 0; i < SIZE; i++) {
        arr1[i] = SIZE * 2 - i - 1;  // descending
        arr2[i] = SIZE - i - 1;      // descending
    }
}

long sum_merged(void) {
    long sum = 0;
    for (int i = 0; i < SIZE * 2; i++) {
        sum += merged[i];
    }
    return sum;
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        init_arrays();
        merge_arrays();
        sum += sum_merged();
    }
    return sum;
}

int main(void) {
    printf("%ld\n", bench());
    return 0;
}
