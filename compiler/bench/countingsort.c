// expected: 49950000
// Counting sort benchmark - sort 100K elements

#include <stdio.h>

#define SIZE 100000
#define RANGE 1000

int arr[SIZE];
int cnt[RANGE];
int out[SIZE];

void counting_sort(void) {
    // Clear counts
    for (int i = 0; i < RANGE; i++) cnt[i] = 0;

    // Count occurrences
    for (int i = 0; i < SIZE; i++) {
        cnt[arr[i]]++;
    }

    // Cumulative count (descending order)
    for (int i = RANGE - 2; i >= 0; i--) {
        cnt[i] += cnt[i + 1];
    }

    // Build output
    for (int i = 0; i < SIZE; i++) {
        out[--cnt[arr[i]]] = arr[i];
    }

    // Copy back
    for (int i = 0; i < SIZE; i++) {
        arr[i] = out[i];
    }
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = i % RANGE;
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
    counting_sort();
    printf("%ld\n", sum_arr());
    return 0;
}
