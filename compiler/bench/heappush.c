// expected: 352
#include <stdio.h>
#define N 100000

long heap[N];
int hsize = 0;

void swap_heap(int i, int j) {
    long t = heap[i];
    heap[i] = heap[j];
    heap[j] = t;
}

void sift_up(int i) {
    while (i > 0) {
        int p = (i - 1) / 2;
        if (heap[i] > heap[p]) {
            swap_heap(i, p);
            i = p;
        } else {
            break;
        }
    }
}

void push(long val) {
    heap[hsize] = val;
    sift_up(hsize);
    hsize++;
}

void init() {
    hsize = 0;
    for (int i = 0; i < N; i++) {
        push(((long)(i % 17) * (i % 23)) % N);
    }
}

int main() {
    init();
    printf("%ld\n", heap[0]);
    return 0;
}
