// expected: 4995000
// Select nth element (quickselect) benchmark - 10K times

#include <stdio.h>

#define SIZE 1000
#define ITERS 10000

int arr[SIZE];
int work[SIZE];

void swap_work(int i, int j) {
    int tmp = work[i];
    work[i] = work[j];
    work[j] = tmp;
}

int partition(int lo, int hi) {
    int pivot = work[(lo + hi) / 2];
    while (lo <= hi) {
        while (work[lo] > pivot) lo++;
        while (work[hi] < pivot) hi--;
        if (lo <= hi) {
            swap_work(lo, hi);
            lo++;
            hi--;
        }
    }
    return lo;
}

int quickselect(int k, int lo, int hi) {
    while (lo < hi) {
        int p = partition(lo, hi);
        if (p == k) return work[p];
        if (p < k) {
            lo = p + 1;
        } else {
            hi = p - 1;
        }
    }
    return work[lo];
}

void copy_to_work(void) {
    for (int i = 0; i < SIZE; i++) {
        work[i] = arr[i];
    }
}

int select_kth(int k) {
    copy_to_work();
    return quickselect(k, 0, SIZE - 1);
}

void init_arr(void) {
    for (int i = 0; i < SIZE; i++) {
        arr[i] = (i * 7 + 13) % SIZE;
    }
}

long bench(void) {
    long sum = 0;
    for (int i = 0; i < ITERS; i++) {
        sum += select_kth(i % SIZE);
    }
    return sum;
}

int main(void) {
    init_arr();
    printf("%ld\n", bench());
    return 0;
}
