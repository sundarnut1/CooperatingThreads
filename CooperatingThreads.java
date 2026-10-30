import java.lang.*;
import java.security.SecureRandom;

public class CooperatingThreads {

    // Shared lock object for synchronization
    private final Object lock = new Object();
    
    // Determines whose turn it is. Randomized to allow either thread to start first.
    private boolean numberTurn = false;
    private SecureRandom random;

    public static void main(String[] args) {

        CooperatingThreads cooperatingThreads = new CooperatingThreads();
        cooperatingThreads.ChooseRandom();
        cooperatingThreads.Run();
    }

    private CooperatingThreads() {
        this.random = new SecureRandom();
    }

    private void ChooseRandom() {

        this.numberTurn = (this.random.nextInt(100) % 2) == 0;
    }

    private void Run() {

        System.out.println("Starting with numbers? " + numberTurn);

        // Thread 1: Prints numbers from 1 to 25 in blocks of 5
        Thread numberThread = new Thread(() -> {
            int currentNum = 1;
            for (int batch = 0; batch < 5; batch++) {
                synchronized (lock) {
                    while (!numberTurn) {
                        try {
                            lock.wait();
                        } catch (InterruptedException e) {
                            Thread.currentThread().interrupt();
                            return;
                        }
                    }

                    // Print 5 numbers
                    for (int i = 0; i < 5; i++) {
                        System.out.print(currentNum++ + " ");
                    }

                    System.out.println();

                    // Hand over control to the letter thread
                    numberTurn = false;
                    lock.notifyAll();
                }
            }
        }, "NumberThread");

        // Thread 2: Prints letters from A to Y in blocks of 5
        Thread letterThread = new Thread(() -> {
            char currentLetter = 'A';
            for (int batch = 0; batch < 5; batch++) {
                synchronized (lock) {
                    while (numberTurn) {
                        try {
                            lock.wait();
                        } catch (InterruptedException e) {
                            Thread.currentThread().interrupt();
                            return;
                        }
                    }

                    // Print 5 letters
                    for (int i = 0; i < 5; i++) {
                        System.out.print(currentLetter++ + " ");
                    }

                    System.out.println();

                    // Hand over control to the number thread
                    numberTurn = true;
                    lock.notifyAll();
                }
            }
        }, "LetterThread");

        // Start both threads
        numberThread.start();
        letterThread.start();
    }
}
