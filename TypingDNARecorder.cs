namespace TypingDNA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using java.math;
    
    public class TypingDNARecorder 
    {
        #region Fields
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
        #endregion

        #region Private methods
        private static void historyAdd(int[] arr) {
            historyStack.Add(arr);
            if (historyStack.Count > maxHistoryLength) {
                historyStack.RemoveAt(0);
            }
        }
    
        private static void historyAddDiagram(int[] arr) {
            stackDiagram.Add(arr);
        }
    
        private static int[] getSeek(int length) {
            int historyTotalLength = historyStack.Count;
            if (length > historyTotalLength) {
                length = historyTotalLength;
            }
            List<int> seekArr = new List<int>();
            for (int i = 1; i <= length; i++) {
                int seekTime = (int) historyStack[(historyTotalLength - i)][1];
                if (seekTime < maxSeekTime && seekTime > 0) {
                    seekArr.Add(seekTime);
                }
            }
            int[] seekList = seekArr.ToArray();
            return seekList;
        }

        private static int[] getPress(int length) {
            int historyTotalLength = historyStack.Count;
            if (length > historyTotalLength) {
                length = historyTotalLength;
            }
            List<int> pressArr = new List<int>();
            for (int i = 1; i <= length; i++) {
                int pressTime = (int) historyStack[historyTotalLength - i][2];
                if (pressTime < maxPressTime && pressTime > 0) {
                    pressArr.Add(pressTime);
                }
            }
            int[] pressList = pressArr.ToArray();
            return pressList;
        }
    
        private static BigInteger fnv1a_32(byte[] data) {
            BigInteger hash = new BigInteger("721b5ad4", 16);
            foreach (byte b in data) {
                hash = hash.xor(BigInteger.valueOf((int) b & 0xff));
                hash = hash.multiply(new BigInteger("01000193", 16)).mod(new BigInteger("2").pow(32));
            }
            return hash;
        }

        private static string getDiagram(bool extended, string str, int textId, int tpLength, bool caseSensitive) 
        {
            string returnArr = "";
            int diagramType = (extended == true) ? 1 : 0;
            int diagramHistoryLength = stackDiagram.Count;
            int missingCount = 0;
            int strLength = tpLength;
            if (str.Length > 0) {
                strLength = str.Length;
            } 
            else if (strLength > diagramHistoryLength || strLength == 0) {
                strLength = diagramHistoryLength;
            }
            String returnTextId = "0";
            if (textId == 0 && str.Length > 0) {
                returnTextId = hash32(str);
            } else {
                returnTextId = "" + textId;
            }
            String returnArr0 = (mobile ? 1 : 0) + "," + version + "," + flags + "," + diagramType + "," + strLength + ","
            + returnTextId + ",0,-1,-1,0,-1,-1,0,-1,-1,0,-1,-1,0,-1,-1";
            returnArr += returnArr0;
            if (str.Length > 0) {
                String strLower = str.ToLower();
                String strUpper = str.ToUpper();
                List<int> lastFoundPos = new List<int>();
                int lastPos = 0;
                int strUpperCharCode;
                int currentSensitiveCharCode;
                for (int i = 0; i < str.Length; i++) {
                    int currentCharCode = (int) str[i];
                    if (!caseSensitive) {
                        strUpperCharCode = (int) strUpper[i];
                        currentSensitiveCharCode = (strUpperCharCode != currentCharCode) ? strUpperCharCode : (int) strLower[i];
                    } else {
                        currentSensitiveCharCode = currentCharCode;
                    }
                    int startPos = lastPos;
                    int finishPos = diagramHistoryLength;
                    bool found = false;
                    while (found == false) {
                        for (int j = startPos; j < finishPos; j++) {
                            int[] arr = stackDiagram[j];
                            int charCode = arr[3];
                            if (charCode == currentCharCode || (!caseSensitive && charCode == currentSensitiveCharCode)) {
                                found = true;
                                if (j == lastPos) {
                                    lastPos++;
                                    lastFoundPos.Clear();
                                } else {
                                    lastFoundPos.Add(j);
                                    int len = lastFoundPos.Count;
                                    if (len > 1 && lastFoundPos[len - 1] == lastFoundPos[len - 2] + 1) {
                                        lastPos = j + 1;
                                        lastFoundPos.Clear();
                                    }
                                }
                                int keyCode = arr[0];
                                int seekTime = arr[1];
                                int pressTime = arr[2];
                                if (extended) {
                                    returnArr += "|" + charCode + "," + seekTime + "," + pressTime + "," + keyCode;
                                } else {
                                    returnArr += "|" + seekTime + "," + pressTime;
                                }
                                break;
                            }
                        }
                        if (found == false) {
                            if (startPos != 0) {
                                startPos = 0;
                                finishPos = lastPos;
                            } else {
                                found = true;
                                if (replaceMissingKeys) {
                                    missingCount++;
                                    int seekTime, pressTime;
                                    if (savedMissingAvgValuesHistoryLength == -1
                                        || savedMissingAvgValuesHistoryLength != diagramHistoryLength) {
                                        int[] histSktF = fo(getSeek(200));
                                        int[] histPrtF = fo(getPress(200));
                                        seekTime = (int) Math.Round(avg(histSktF));
                                        pressTime = (int) Math.Round(avg(histPrtF));
                                        savedMissingAvgValuesSeekTime = seekTime;
                                        savedMissingAvgValuesPressTime = pressTime;
                                        savedMissingAvgValuesHistoryLength = diagramHistoryLength;
                                    } else {
                                        seekTime = savedMissingAvgValuesSeekTime;
                                        pressTime = savedMissingAvgValuesPressTime;
                                    }
                                    int missing = 1;
                                    if (extended) {
                                        returnArr += "|" + currentCharCode + "," + seekTime + "," + pressTime + ","
                                        + currentCharCode + "," + missing;
                                    } else {
                                        returnArr += "|" + seekTime + "," + pressTime + "," + missing;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if (replaceMissingKeysPerc < missingCount * 100 / strLength) {
                        returnArr = returnArr0;
                        return null;
                    }
                }
            } else {
                int startCount = 0;
                if (tpLength > 0) {
                    startCount = diagramHistoryLength - tpLength;
                }
                if (startCount < 0) {
                    startCount = 0;
                }
                for (int i = startCount; i < diagramHistoryLength; i++) {
                    int[] arr = stackDiagram[i];
                    int keyCode = arr[0];
                    int seekTime = arr[1];
                    int pressTime = arr[2];
                    if (extended) {
                        int charCode = arr[3];
                        returnArr += "|" + charCode + "," + seekTime + "," + pressTime + "," + keyCode;
                    } else {
                        returnArr += "|" + seekTime + "," + pressTime;
                    }
                }
            }
            return returnArr;
        }

        private static String get(int length) 
        {
            int historyTotalLength = historyStack.Count;
            if (length == 0) {
                length = defaultHistoryLength;
            }
            if (length > historyTotalLength) {
                length = historyTotalLength;
            }
            Dictionary<int, List<int>> historyStackObjSeek = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> historyStackObjPress = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> historyStackObjPrev = new Dictionary<int, List<int>>();
            for (int i = 1; i <= length; i++) 
            {
                int[] __arr = historyStack[historyTotalLength - i];
                int keyCode = __arr[0];
                int seekTime = __arr[1];
                int pressTime = __arr[2];
                int prevKeyCode = __arr[3];
                if (keyCodesObj[keyCode] == 1) {
                    if (seekTime <= maxSeekTime) {
                        List<int> sarr = historyStackObjSeek[keyCode];
                        if (sarr == null) {
                            sarr = new List<int>();
                        }
                        sarr.Add(seekTime);
                        historyStackObjSeek.Put(keyCode, sarr);
                        if (prevKeyCode != 0) {
                            if (keyCodesObj[prevKeyCode] == 1) {
                                List<int> poarr = historyStackObjPrev[prevKeyCode];
                                if (poarr == null) {
                                    poarr = new List<int>();
                                }
                                poarr.Add(seekTime);
                                historyStackObjPrev.Put(prevKeyCode, poarr);
                            }
                        }
                    }
                    if (pressTime <= maxPressTime) {
                        List<int> prarr = historyStackObjPress[keyCode];
                        if (prarr == null) {
                            prarr = new List<int>();
                        }
                        prarr.Add(pressTime);
                        historyStackObjPress.Put(keyCode, prarr);
                    }
                }
            }
            Dictionary<int, List<double>> meansArr = new Dictionary<int, List<double>>();
            double zl = 0.0000001;
            int histRev = length;
            int[] histSktF = fo(getSeek(length));
            int[] histPrtF = fo(getPress(length));
            double pressHistMean = Math.Round(avg(histPrtF));
            if (pressHistMean.IsNaN() || pressHistMean.IsInfinite()) {
                pressHistMean = 0.0;
            }
            double seekHistMean = Math.Round(avg(histSktF));
            if (seekHistMean.IsNaN() || seekHistMean.IsInfinite()) {
                seekHistMean = 0.0;
            }
            double pressHistSD = Math.Round(sd(histPrtF));
            if (pressHistSD.IsNaN() || pressHistSD.IsInfinite()) {
                pressHistSD = 0.0;
            }
            double seekHistSD = Math.Round(sd(histSktF));
            if (seekHistSD.IsNaN() || seekHistSD.IsInfinite()) {
                seekHistSD = 0.0;
            }
            double charMeanTime = seekHistMean + pressHistMean;
            double pressRatio = rd((pressHistMean + zl) / (charMeanTime + zl));
            double seekToPressRatio = rd((1 - pressRatio) / pressRatio);
            double pressSDToPressRatio = rd((pressHistSD + zl) / (pressHistMean + zl));
            double seekSDToPressRatio = rd((seekHistSD + zl) / (pressHistMean + zl));
            int cpm = (int) Math.Round(6E4 / (charMeanTime + zl));
            if (charMeanTime == 0) {
                cpm = 0;
            }
            for (int i = 0; i < keyCodes.Length; i++) 
            {
                int keyCode = keyCodes[i];
                List<int> sarr = historyStackObjSeek[keyCode];
                List<int> prarr = historyStackObjPress[keyCode];
                List<int> poarr = historyStackObjPrev[keyCode];
                int srev = 0;
                int prrev = 0;
                int porev = 0;
                if (sarr != null) {
                    srev = sarr.Count;
                }
                if (prarr != null) {
                    prrev = prarr.Count;
                }
                if (poarr != null) {
                    porev = poarr.Count;
                }
                int rev = prrev;
                double seekMean = 0.0;
                double pressMean = 0.0;
                double postMean = 0.0;
                double seekSD = 0.0;
                double pressSD = 0.0;
                double postSD = 0.0;
                switch (srev) {
                    case 0:
                        break;
                    case 1:
                        seekMean = rd((sarr[0] + zl) / (seekHistMean + zl));
                        break;
                    default:
                        int[] newArr = sarr.ToArray();
                        int[] _arr = fo(newArr);
                        seekMean = rd((avg(_arr) + zl) / (seekHistMean + zl));
                        seekSD = rd((sd(_arr) + zl) / (seekHistSD + zl));
                        break;
                }
                switch (prrev) {
                    case 0:
                        break;
                    case 1:
                        pressMean = rd((prarr[0] + zl) / (pressHistMean + zl));
                        break;
                    default:
                        int[] newArr = prarr.ToArray();
                        int[] _arr = fo(newArr);
                        pressMean = rd((avg(_arr) + zl) / (pressHistMean + zl));
                        pressSD = rd((sd(_arr) + zl) / (pressHistSD + zl));
                        break;
                }
                switch (porev) {
                    case 0:
                        break;
                    case 1:
                        postMean = rd((poarr[0] + zl) / (seekHistMean + zl));
                        break;
                    default:
                        int[] newArr = poarr.ToArray();
                        int[] _arr = fo(newArr);
                        postMean = rd((avg(_arr) + zl) / (seekHistMean + zl));
                        postSD = rd((sd(_arr) + zl) / (seekHistSD + zl));
                        break;
                }
                List<double> varr = new List<double>();
                varr.Add((double) rev);
                varr.Add(seekMean);
                varr.Add(pressMean);
                varr.Add(postMean);
                varr.Add(seekSD);
                varr.Add(pressSD);
                varr.Add(postSD);
                meansArr.Put((int) keyCode, varr);
            }
            List<object> arr = new List<object>();
            arr.Add(histRev);
            arr.Add(cpm);
            arr.Add((int) (double) charMeanTime);
            arr.Add(pressRatio);
            arr.Add(seekToPressRatio);
            arr.Add(pressSDToPressRatio);
            arr.Add(seekSDToPressRatio);
            arr.Add(pressHistMean);
            arr.Add(seekHistMean);
            arr.Add(pressHistSD);
            arr.Add(seekHistSD);
            for (int c = 0; c <= 6; c++) 
            {
                for (int i = 0; i < keyCodes.Length; i++) 
                {
                    int keyCode = keyCodes[i];
                    List<double> varr = new List<double>();
                    varr = meansArr[keyCode];
                    double val = varr[c];
                    if (((double) (double) (val)).IsNaN()) {
                        val = 0.0;
                    }
                    if (val == 0 && c > 0) {
                        val = 1;
                        arr.Add((int) val);
                    } else if (c == 0) {
                        arr.Add((int) val);
                    } else {
                        arr.Add((double) val);
                    }
                }
            }
            int mobile = 0;
            arr.Add(mobile);
            string typingPattern = arr.Select(s => s.ToString()).Aggregate((a,b) => a + b);
            typingPattern = typingPattern.Substring(1, typingPattern.Length - 1);
            return typingPattern;
        }
    
        private static long getTime() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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

        private static double rd(double value, int places) {

            if (places < 0)

                throw new ArgumentException();

            

            BigDecimal bd = new BigDecimal(value);

            bd = bd.setScale(places, RoundingMode.HALF_UP);

            return bd.doubleValue();

        }

        private static double rd(double value) {

            return rd(value, 4);

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

        #endregion
        
        #region Public methods
        public static String hash32(String str) {
            str = str.ToLower();
            return fnv1a_32(System.Text.UTF8Encoding.UTF8.GetBytes(str)).toString();
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
        
        public static void keyPressed(int keyCode, char keyChar, bool modifiers) {
            long t0 = pt1;
            pt1 = getTime();
            int seekTotal = (int) (pt1 - t0);
            long startTime = pt1;
            if(keyCode >= maxKeyCode) {
                return;
            }
            if (recording == true && !modifiers) {
                if (keyCodesObj[keyCode] == 1) {
                    wfk[keyCode] = 1;
                    skt[keyCode] = seekTotal;
                    sti[keyCode] = startTime;
                }
            }
            if (diagramRecording == true && (java.lang.Character.isDefined(keyChar))) {
                lastPressedKey = keyCode;
                dwfk[keyCode] = 1;
                dskt[keyCode] = seekTotal;
                dsti[keyCode] = startTime;
                drkc[keyCode] = keyChar;
            }
        }
    
        public static void keyTyped(char keyChar) {
            if (diagramRecording == true && (java.lang.Character.isDefined(keyChar)) && lastPressedKey < maxKeyCode ) {
                drkc[lastPressedKey] = (int)keyChar;
            }
        }
        
        #endregion
    }
}