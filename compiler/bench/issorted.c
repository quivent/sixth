// expected: 5000
// Check if array is sorted benchmark - 10K iterations

#include <stdio.h>

#define SIZE 1000
#define ITERS 10000

int arr[SIZE];

int is_sorted(void) {
    for (int i = 1; i < SIZE; i++) {
        if (arr[i - 1] < arr[i]) return 0;
    }
    return 1;
}

void init_sorted(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = SIZE - i - 1;
    }
}

void init_unsorted(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = (i * 7) % SIZE;
    }
}

int bench(void) {
    int count = 0;
    for (int i = 0; i < ITERS; i++) {
        if (i % 2) init_unsorted();
        else init_sorted();
        if (is_sorted()) count++;
    }
    return count;
}

int main(void) {
    printf("%d\n", bench());
    return 0;
}
