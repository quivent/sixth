\ expect: -1
\ category: sixth-word
\ reason: FIND can locate word defined in same compilation
\ Test find can find main itself
: main s" main" find . drop cr ;
