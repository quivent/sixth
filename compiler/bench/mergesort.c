// expected: 49995000
// Mergesort benchmark - sort 10000 elements, sum result

#include <stdio.h>

#define SIZE 10000

int arr[SIZE];
int tmp[SIZE];

void merge(int lo, int mid, int hi) {
    int i = lo, j = mid + 1, k = lo;
    while (k <= hi) {
        if (i <= mid && (j > hi || arr[i] >= arr[j])) {
            tmp[k++] = arr[i++];
        } else {
            tmp[k++] = arr[j++];
        }
    }
    for (k = lo; k <= hi; k++) {
        arr[k] = tmp[k];
    }
}

void msort(int lo, int hi) {
    if (lo < hi) {
        int mid = (lo + hi) / 2;
        msort(lo, mid);
        msort(mid + 1, hi);
        merge(lo, mid, hi);
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
    msort(0, SIZE - 1);
    printf("%ld\n", sum_arr());
    return 0;
}
