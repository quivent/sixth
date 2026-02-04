// expected: 352
#include <stdio.h>
#define N 100000

long heap[N];

void swap_heap(int i, int j) {
    long t = heap[i];
    heap[i] = heap[j];
    heap[j] = t;
}

void sift_down(int i) {
    while (2 * i + 1 < N) {
        int j = 2 * i + 1;
        if (j + 1 < N && heap[j] < heap[j + 1]) j++;
        if (heap[i] < heap[j]) {
            swap_heap(i, j);
            i = j;
        } else {
            break;
        }
    }
}

void heapify() {
    for (int i = N / 2 - 1; i >= 0; i--) {
        sift_down(i);
    }
}

void init_heap() {
    for (int i = 0; i < N; i++) {
        heap[i] = ((long)(i % 17) * (i % 23)) % N;
    }
}

int main() {
    init_heap();
    heapify();
    printf("%ld\n", heap[0]);
    return 0;
}
