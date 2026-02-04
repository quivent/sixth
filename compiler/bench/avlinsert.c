// expected: 99994
#include <stdio.h>
#define N 50000

int left_child[N];
int right_child[N];
int ht[N];
int key[N];
int root = -1;
int node_count = 0;

int height(int node) {
    return (node < 0) ? 0 : ht[node];
}

int max(int a, int b) { return (a > b) ? a : b; }

void update_ht(int node) {
    ht[node] = 1 + max(height(left_child[node]), height(right_child[node]));
}

int balance(int node) {
    return height(left_child[node]) - height(right_child[node]);
}

int rotate_right(int y) {
    int x = left_child[y];
    left_child[y] = right_child[x];
    update_ht(y);
    right_child[x] = y;
    update_ht(x);
    return x;
}

int rotate_left(int x) {
    int y = right_child[x];
    right_child[x] = left_child[y];
    update_ht(x);
    left_child[y] = x;
    update_ht(y);
    return y;
}

int rebalance(int node) {
    int bf = balance(node);
    if (bf > 1) {
        if (balance(left_child[node]) < 0) {
            left_child[node] = rotate_left(left_child[node]);
        }
        return rotate_right(node);
    }
    if (bf < -1) {
        if (balance(right_child[node]) > 0) {
            right_child[node] = rotate_right(right_child[node]);
        }
        return rotate_left(node);
    }
    update_ht(node);
    return node;
}

int avl_insert(int val, int node) {
    if (node < 0) {
        key[node_count] = val;
        left_child[node_count] = -1;
        right_child[node_count] = -1;
        ht[node_count] = 1;
        return node_count++;
    }
    if (val < key[node]) {
        left_child[node] = avl_insert(val, left_child[node]);
    } else {
        right_child[node] = avl_insert(val, right_child[node]);
    }
    return rebalance(node);
}

int lcg(int n) {
    return (n * 1103515245 + 12345) & 2147483647;
}

void build_tree() {
    root = -1;
    node_count = 0;
    int seed = 12345;
    for (int i = 0; i < N; i++) {
        seed = lcg(seed);
        root = avl_insert(seed % N, root);
    }
}

long sum_heights() {
    long sum = 0;
    for (int i = 0; i < N; i++) {
        sum += ht[i];
    }
    return sum;
}

int main() {
    build_tree();
    printf("%ld\n", sum_heights());
    return 0;
}
