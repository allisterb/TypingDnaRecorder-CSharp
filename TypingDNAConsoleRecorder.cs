namespace  TypingDNA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using java.math;
    
    public class TypingDNAConsoleRecorder {
        public static bool mobile = false;
        public static int maxHistoryLength = 500;
        public static bool replaceMissingKeys = true;
        public static int replaceMissingKeysPerc = 7;
        public static bool recording = true;
        public static bool diagramRecording = true;
        public static double version = 2.14; // (without MOUSE tracking and without special keys)
        
        private static int flags = 1; // JAVA version has flag=1
        private static int maxSeekTime = 1500;
        private static int maxPressTime = 300;
        private static  int[] keyCodes = new int[] { 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
        81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 32, 222, 44, 46, 59, 61, 45, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56,
        57 };
        private static int maxKeyCode = 250;
        private static int defaultHistoryLength = 160;
        private static int[] keyCodesObj = new int[maxKeyCode];
        private static int[] wfk = new int[maxKeyCode];
        private static long[] sti = new long[maxKeyCode];
        private static int[] skt = new int[maxKeyCode];
        private static int[] dwfk = new int[maxKeyCode];
        private static long[] dsti = new long[maxKeyCode];
        private static int[] dskt = new int[maxKeyCode];
        private static int[] drkc = new int[maxKeyCode];
        private static long pt1;
        private static int prevKeyCode = 0;
        private static int lastPressedKey = 0;
        private static List<int[]> historyStack = new List<int[]>();
        private static List<int[]> stackDiagram = new List<int[]>();
        private static int savedMissingAvgValuesHistoryLength = -1;
        private static int savedMissingAvgValuesSeekTime;
        private static int savedMissingAvgValuesPressTime;

        private static long getTime() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();   
        private static double rd(double value, int places) {

        if (places < 0)

            throw new ArgumentException();

        

        BigDecimal bd = new BigDecimal(value);

        bd = bd.setScale(places, RoundingMode.HALF_UP);

        return bd.doubleValue();

    }
        private static int[] fo(int[] arr) {

        int len = (int) arr.Length;

        if (len > 1) {

            Array.Sort(arr);

            double asd = sd(arr);

            double aMean = arr[(int) System.Math.Ceiling(len / 2.0)];

            double multiplier = 2.0;

            double maxVal = aMean + multiplier * asd;

            double minVal = aMean - multiplier * asd;

            if (len < 20) {

                minVal = 0;

            }

            List<int> fVal = new List<int>();

            for (int i = 0; i < len; i++) {

                int tempval = arr[i];

                if (tempval < maxVal && tempval > minVal) {

                    fVal.Add(tempval);

                }

            }

            int[] newArr = fVal.ToArray();

            return newArr;

        } else {

            return arr;

        }

    }

    

    private static Double avg(int[] arr) {

        int len = (int) arr.Length;

        if (len > 0) {

            Double sum = 0.0;

            for (int i = 0; i < len; i++) {

                sum += arr[i];

            }

            return rd(sum / ((double) len));

        } else {

            return 0.0;

        }

    }

    private static double rd(double value) {

        return rd(value, 4);

    }

    private static double sd(int[] arr) {

        int len = (int) arr.Length;

        if (len < 2) {

            return 0.0;

        } else {

            double sumVS = 0;

            double mean = avg(arr);

            for (int i = 0; i < len; i++) {

                double numd = (double) arr[i] - mean;

                sumVS += numd * numd;

            }

            return Math.Sqrt(sumVS / ((double) len));

        }

    }
        
        /**

     * Resets the history stack of recorded typing events.

     */

        public static void reset() {

            historyStack = new List<int[]>();

            stackDiagram = new List<int[]>();

        }

        

        /**

        * Automatically called at initialization. It starts the recording of typing events.

        * You only have to call .start() to resume recording after a .stop()

        */

        public static void start() {

            recording = true;

            diagramRecording = true;

        }

        

        /**

        * Ends the recording of further typing events.

        */

        public static void stop() {

            recording = false;

            diagramRecording = false;

        }

        public static void initialize() {

        for (int i = 0; i < keyCodes.Length; i++) {

            keyCodesObj[(int) keyCodes[i]] = 1;

        }

        pt1 = getTime();

        reset();

        start();

        }
    }
}