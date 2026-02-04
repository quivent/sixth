// expected: 92
#include <stdio.h>
#include <stdlib.h>
int board[8];
long solutions = 0;

int safe(int row, int col) {
    for (int i = 0; i < col; i++) {
        if (board[i] == row) return 0;
        if (abs(board[i] - row) == abs(i - col)) return 0;
    }
    return 1;
}

void queens(int col) {
    if (col == 8) { solutions++; return; }
    for (int row = 0; row < 8; row++) {
        if (safe(row, col)) {
            board[col] = row;
            queens(col + 1);
        }
    }
}

int main() {
    queens(0);
    printf("%ld\n", solutions);
    return 0;
}
