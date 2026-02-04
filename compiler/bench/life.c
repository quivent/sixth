// expected: 5
// Game of Life - run simulation steps
#include <stdio.h>
#include <string.h>

#define SIZE 32

char grid[SIZE * SIZE];
char next_grid[SIZE * SIZE];

int wrap(int n) { return (n + SIZE) % SIZE; }

int g(int x, int y) { return grid[y * SIZE + x]; }
void gs(int x, int y, int v) { grid[y * SIZE + x] = v; }
void ns(int x, int y, int v) { next_grid[y * SIZE + x] = v; }

int count_neighbors(int x, int y) {
    int count = 0;
    for (int dy = -1; dy <= 1; dy++) {
        for (int dx = -1; dx <= 1; dx++) {
            if (dx == 0 && dy == 0) continue;
            count += g(wrap(x + dx), wrap(y + dy));
        }
    }
    return count;
}

void step_cell(int x, int y) {
    int n = count_neighbors(x, y);
    int alive = g(x, y);
    int next;
    if (alive) {
        next = (n == 2 || n == 3) ? 1 : 0;
    } else {
        next = (n == 3) ? 1 : 0;
    }
    ns(x, y, next);
}

void step(void) {
    for (int y = 0; y < SIZE; y++) {
        for (int x = 0; x < SIZE; x++) {
            step_cell(x, y);
        }
    }
    memcpy(grid, next_grid, SIZE * SIZE);
}

int count_alive(void) {
    int count = 0;
    for (int i = 0; i < SIZE * SIZE; i++) {
        count += grid[i];
    }
    return count;
}

void init_glider(void) {
    memset(grid, 0, SIZE * SIZE);
    gs(2, 1, 1); gs(3, 2, 1); gs(1, 3, 1); gs(2, 3, 1); gs(3, 3, 1);
}

int main(void) {
    init_glider();
    for (int i = 0; i < 1000; i++) step();
    printf("%d\n", count_alive());
    return 0;
}
