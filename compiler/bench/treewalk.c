// expected: 4999950000
#include <stdio.h>
#define N 100000

int left_child[N];
int right_child[N];
int val[N];
int stack[N];
int sp;
long sum;

void init_tree() {
    for (int i = 0; i < N; i++) {
        val[i] = i;
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

void inorder() {
    sum = 0;
    sp = 0;
    int curr = 0;
    while (curr >= 0) {
        stack[sp++] = curr;
        curr = left_child[curr];
    }
    while (sp > 0) {
        curr = stack[--sp];
        sum += val[curr];
        curr = right_child[curr];
        while (curr >= 0) {
            stack[sp++] = curr;
            curr = left_child[curr];
        }
    }
}

int main() {
    init_tree();
    inorder();
    printf("%ld\n", sum);
    return 0;
}
