using System;
using System.Security.Cryptography;
using System.Threading;

public class Program {
    private readonly object lockObj = new object();
    private bool isNumberTurn = true;
    private int numberCurrent = 1;
    private char letterCurrent = 'A';

    static void Main(string[] args) {
        var program = new Program();

        Console.WriteLine("Starting thread cooperation (5 elements at a time):\n");

        program.SeedRandomStart();

        Thread numberThread = new Thread(program.PrintNumbers);
        Thread letterThread = new Thread(program.PrintLetters);

        // Start both threads
        numberThread.Start();
        letterThread.Start();

        // Wait for both threads to finish
        numberThread.Join();
        letterThread.Join();

        Console.WriteLine("\n\nExecution finished!");
    }

    private void SeedRandomStart() {
        // Randomly decide which thread starts first
        using (var rng = RandomNumberGenerator.Create()) {
            byte[] randomByte = new byte[1];
            rng.GetBytes(randomByte);
            this.isNumberTurn = (randomByte[0] % 2 == 0);
        }
    }

    private void PrintNumbers() {
        while (this.numberCurrent <= 25) {
            lock (this.lockObj) {
                // Wait while it's not the number thread's turn
                while (!this.isNumberTurn && this.numberCurrent <= 25) {
                    Monitor.Wait(this.lockObj);
                }

                if (this.numberCurrent > 25)
                    break;

                // Print up to 5 numbers
                for (int i = 0; i < 5 && this.numberCurrent <= 25; i++) {
                    Console.Write($"{this.numberCurrent} ");
                    numberCurrent++;
                }

                Console.WriteLine(); // New line after printing numbers

                // Hand off control to the letter thread
                this.isNumberTurn = false;
                Monitor.PulseAll(this.lockObj);
            }
        }

        Console.WriteLine("\nFinished with numbers thread");
    }

    private void PrintLetters() {
        while (this.letterCurrent <= 'Y') {
            lock (this.lockObj) {
                // Wait while it's the number thread's turn, UNLESS numbers are done and 'Z' needs printing
                while (this.isNumberTurn && this.numberCurrent <= 25) {
                    Monitor.Wait(this.lockObj);
                }

                // Print up to 5 letters
                for (int i = 0; i < 5 && this.letterCurrent <= 'Z'; i++) {
                    Console.Write($"{this.letterCurrent} ");
                    this.letterCurrent++;
                }

                Console.WriteLine(); // New line after printing letters

                // Hand off control back to the number thread
                this.isNumberTurn = true;
                Monitor.PulseAll(lockObj);
            }
        }

        Console.WriteLine("\nFinished with letters thread");
    }
}
