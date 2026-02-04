// expected: 4999950000
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

void sift_down(int i) {
    while (2 * i + 1 < hsize) {
        int j = 2 * i + 1;
        if (j + 1 < hsize && heap[j] < heap[j + 1]) j++;
        if (heap[i] < heap[j]) {
            swap_heap(i, j);
            i = j;
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

long pop() {
    long val = heap[0];
    hsize--;
    heap[0] = heap[hsize];
    sift_down(0);
    return val;
}

void init() {
    hsize = 0;
    for (int i = 0; i < N; i++) {
        push(i);
    }
}

long pop_all() {
    long sum = 0;
    for (int i = 0; i < N; i++) {
        sum += pop();
    }
    return sum;
}

int main() {
    init();
    printf("%ld\n", pop_all());
    return 0;
}
