// expected: 1
// Mutual tail recursion - even/odd via mutual calls
#include <stdio.h>

int odd_(long n);
int even_(long n) {
    if (n == 0) return 1;
    return odd_(n - 1);
}
int odd_(long n) {
    if (n == 0) return 0;
    return even_(n - 1);
}

int main() {
    printf("%d\n", even_(10000000));
    return 0;
}
