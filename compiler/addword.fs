\ addword.fs - Add a word to lib/core.fs
\ Usage: fifth compiler/addword.fs name stack body
\ Example: fifth compiler/addword.fs nip "( a b -- b )" "swap drop"

require lib/str.fs

: usage ( -- )
  ." Usage: fifth compiler/addword.fs <name> <stack> <body>" cr
  ." Example: fifth compiler/addword.fs nip '( a b -- b )' 'swap drop'" cr
  bye ;

: main ( -- )
  \ argv: 0=fifth 1=script 2=name 3=stack 4=body
  argc 5 < if usage then

  \ Build: echo ': name stack body ;' >> lib/core.fs
  str-reset
  s" echo ': " str+
  2 argv str+
  s"  " str+
  3 argv str+
  s"  " str+
  4 argv str+
  s"  ;' >> lib/core.fs" str+
  str$ system

  ." Added: : " 2 argv type ."  " 3 argv type ."  " 4 argv type ."  ;" cr ;

main
bye
