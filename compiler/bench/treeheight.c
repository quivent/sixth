// expected: 16
#include <stdio.h>
#define N 100000

int left_child[N];
int right_child[N];
int height[N];

void init_tree() {
    for (int i = 0; i < N; i++) {
        left_child[i] = -1;
        right_child[i] = -1;
    }
    for (int i = 1; i < N; i++) {
        int parent = (i - 1) / 2;
        if (left_child[parent] < 0) {
            left_child[parent] = i;
        } else {
            right_child[parent] = i;
        }
    }
}

void compute_heights() {
    for (int i = 0; i < N; i++) height[i] = 0;
    for (int i = N - 1; i >= 0; i--) {
        int lh = (left_child[i] >= 0) ? height[left_child[i]] + 1 : 0;
        int rh = (right_child[i] >= 0) ? height[right_child[i]] + 1 : 0;
        height[i] = (lh > rh) ? lh : rh;
    }
}

int main() {
    init_tree();
    compute_heights();
    printf("%d\n", height[0]);
    return 0;
}
