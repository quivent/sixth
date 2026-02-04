// expected: 49995000
// Quicksort benchmark - sort 10000 elements, sum result

#include <stdio.h>

#define SIZE 10000

int arr[SIZE];

void swap(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

int partition(int lo, int hi) {
    int pivot = arr[(lo + hi) / 2];
    while (lo <= hi) {
        while (arr[lo] > pivot) lo++;
        while (arr[hi] < pivot) hi--;
        if (lo <= hi) {
            swap(lo, hi);
            lo++;
            hi--;
        }
    }
    return lo;
}

void qsort_impl(int lo, int hi) {
    if (lo < hi) {
        int p = partition(lo, hi);
        qsort_impl(lo, p - 1);
        qsort_impl(p, hi);
    }
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
    qsort_impl(0, SIZE - 1);
    printf("%ld\n", sum_arr());
    return 0;
}
