#include <stdio.h>
#include <stdint.h>

uint32_t fib(uint32_t n) {
    if (n < 2) return n;
    return fib(n-1) + fib(n-2);
}

int main() {
    printf("%u\n", fib(45));
    return 0;
}
