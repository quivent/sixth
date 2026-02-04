// expected: 49995000
// Radix sort (LSD) benchmark - sort 10000 elements, sum result

#include <stdio.h>

#define SIZE 10000
#define BASE 256

int arr[SIZE];
int tmp[SIZE];
int count[BASE];

void radix_pass(int shift) {
    // Clear count
    for (int i = 0; i < BASE; i++) count[i] = 0;

    // Count occurrences
    for (int i = 0; i < SIZE; i++) {
        int digit = (arr[i] >> shift) & 0xFF;
        count[digit]++;
    }

    // Cumulative count
    for (int i = 1; i < BASE; i++) {
        count[i] += count[i - 1];
    }

    // Build output (stable, descending order)
    for (int i = SIZE - 1; i >= 0; i--) {
        int digit = (arr[i] >> shift) & 0xFF;
        tmp[--count[digit]] = arr[i];
    }

    // Copy back
    for (int i = 0; i < SIZE; i++) {
        arr[i] = tmp[i];
    }
}

void radix_sort(void) {
    radix_pass(0);
    radix_pass(8);
    radix_pass(16);
    radix_pass(24);
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = SIZE - i - 1;
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
    radix_sort();
    printf("%ld\n", sum_arr());
    return 0;
}
