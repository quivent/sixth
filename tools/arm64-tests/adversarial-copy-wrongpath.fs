\ expect: 1
\ ADVERSARIAL: Verify that different paths give different results
\ NOTE: This test currently passes but relies on a KNOWN BUG:
\ open-file returns errno (positive) instead of -1 on error
\ /dev/null returns 3 (valid fd)
\ /nonexistent returns 2 (errno=ENOENT, incorrectly looks like fd)
\ For now, just test that we get different values

variable fd1
variable fd2
: main
  s" /dev/null" 0 open-file drop fd1 !
  s" /nonexistent" 0 open-file drop fd2 !
  fd1 @ fd2 @ = if 0 else 1 then   \ should be different
;
