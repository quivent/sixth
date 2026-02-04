// expected: 724
#include <stdio.h>
#include <stdlib.h>
int board10[10];
long solutions10 = 0;

int safe10(int row, int col) {
    for (int i = 0; i < col; i++) {
        if (board10[i] == row) return 0;
        if (abs(board10[i] - row) == abs(i - col)) return 0;
    }
    return 1;
}

void queens10(int col) {
    if (col == 10) { solutions10++; return; }
    for (int row = 0; row < 10; row++) {
        if (safe10(row, col)) {
            board10[col] = row;
            queens10(col + 1);
        }
    }
}

int main() {
    queens10(0);
    printf("%ld\n", solutions10);
    return 0;
}
