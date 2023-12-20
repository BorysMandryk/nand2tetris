// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.

//  
//  while true:
//      fill = 0
//      if OUTPUT != 0:
//          fill = 1
//
//      addr = SCREEN
//      for r in range(0, 256):
//          for c in range(0, 32)
//              addr = addr + c
//              A = addr
//              RAM[A] = fill 

    @256
    D = A
    @row_num
    M = D
    @32
    D = A
    @col_num
    M = D

(START)
    @fill
    M = 0
    @KBD
    D = M
    @FILL_LOOP
    D; JEQ

    @fill
    M = -1

(FILL_LOOP)
    @SCREEN
    D = A
    @addr
    M = D
    @r
    M = 0

(ROW_LOOP)
    @c
    M = 0

(COL_LOOP)
    @fill
    D = M
    @addr
    A = M 
    M = D
    @addr
    M = M + 1


    @c
    MD = M + 1
    @col_num
    D = D - M
    @COL_LOOP
    D; JLT
    
    @r
    MD = M + 1
    @row_num
    D = D - M
    @START
    D; JEQ

    @ROW_LOOP
    0; JMP



