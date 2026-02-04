// expected: 1048575
#include <stdio.h>
long move_count = 0;
void hanoi(int n, int from, int to, int aux) {
    if (n == 0) return;
    hanoi(n - 1, from, aux, to);
    move_count++;
    hanoi(n - 1, aux, to, from);
}
int main() {
    hanoi(20, 1, 3, 2);
    printf("%ld\n", move_count);
    return 0;
}
