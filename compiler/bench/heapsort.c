// expected: 49995000
// Heapsort benchmark - sort 10000 elements, sum result

#include <stdio.h>

#define SIZE 10000

int arr[SIZE];

void swap(int i, int j) {
    int tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
}

void sift_down(int start, int end) {
    while (1) {
        int child = 2 * start + 1;
        if (child > end) return;
        if (child + 1 <= end && arr[child] < arr[child + 1]) {
            child++;
        }
        if (arr[start] >= arr[child]) return;
        swap(start, child);
        start = child;
    }
}

void heapify(void) {
    for (int start = SIZE / 2 - 1; start >= 0; start--) {
        sift_down(start, SIZE - 1);
    }
}

void hsort(void) {
    heapify();
    for (int end = SIZE - 1; end > 0; end--) {
        swap(0, end);
        sift_down(0, end - 1);
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
    hsort();
    printf("%ld\n", sum_arr());
    return 0;
}
