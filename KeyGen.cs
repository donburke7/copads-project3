using System.Numerics;
using System.Security.Cryptography;
using Newtonsoft.Json;

/// <summary>
/// Donald Burke
/// CSCI 251
/// </summary>
namespace Messenger {

    /// <summary>
    /// Responsible for generating both public and private RSA keys
    /// </summary>
    public class KeyGenerator {
        
        /// <summary>
        /// Represents size of the keys in bits
        /// </summary>
        private int keySize;

        /// <summary>
        /// KeyGenerator constructor
        /// </summary>
        /// <param name="keySize">Key size in bits taken from user input</param>
        public KeyGenerator(int keySize) {
            this.keySize = keySize;
        }
        
        /// <summary>
        /// Driver method that calls createPubKey and createPrivKey and
        /// uses the PrimeGen class to generate prime numbers for values needed
        /// to generate the public and private keys
        /// </summary>
        public void genKeys() {
            int variance = keySize / 4;
            int pBitSize = this.keySize / 2 - variance;
            int qBitSize = keySize - pBitSize;
            // Generate a p and q of bit size pBitSize and qBitSize
            BigInteger p = PrimeGen.PrimeNumberGenerator(pBitSize);
            BigInteger q = PrimeGen.PrimeNumberGenerator(qBitSize);
            BigInteger r = (p - 1) * (q - 1);
            // Generate N, E and D values
            BigInteger N = p * q;
            BigInteger E = PrimeGen.PrimeNumberGenerator(16);
            BigInteger D = modInverse(E, r);
            // Convert them into byte arrays
            byte[] EByteArr = E.ToByteArray();
            byte[] DByteArr = D.ToByteArray();
            byte[] NByteArr = N.ToByteArray();
            // Gather the size of their byte arrays and assign them to
            // e, d and n respectively
            int e = EByteArr.Length;
            int d = DByteArr.Length;
            int n = NByteArr.Length;
            // Call key generation methods
            createPubKey(keySize, EByteArr, e, NByteArr, n);
            createPrivKey(keySize, DByteArr, d, NByteArr, n);
        }

        /// <summary>
        /// Creates a public key and stores it into a file called public.key
        /// </summary>
        /// <param name="keySize">Size of the key in bits</param>
        /// <param name="EByteArr">E value as a byte array</param>
        /// <param name="e">The size of EByteArr</param>
        /// <param name="NByteArr">N value as a byte array</param>
        /// <param name="n">The size of NByteArr</param>
        public static void createPubKey(int keySize, byte[] EByteArr, int e, byte[] NByteArr, int n) {
            // Use parameter values to create a public key represented as a byte array
            byte[] pubKeyByteArr = new byte[8 + e + n];
            Array.Copy(NByteArr, 0, pubKeyByteArr, 8 + e, n);
            byte[] nByteArr = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) {
                    Array.Reverse(nByteArr);
            }
            Array.Copy(nByteArr, 0, pubKeyByteArr, 4 + e, 4);
            Array.Copy(EByteArr, 0, pubKeyByteArr, 4, e);
            byte[] eByteArr = BitConverter.GetBytes(e);
            if (BitConverter.IsLittleEndian) {
                    Array.Reverse(eByteArr);
            }
            Array.Copy(eByteArr, 0, pubKeyByteArr, 0, 4);

            // Create the public key object and store the string form of 
            // pubKeyByteArr as the key
            PubKeyObject publicKey = new PubKeyObject();
            publicKey.Key = Convert.ToBase64String(pubKeyByteArr);
            // Store the key in the public.key file
            try {
                using (StreamWriter outputFile = 
                    new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "public.key"))) {
                        outputFile.Write(JsonConvert.SerializeObject(publicKey));
                    }
            } catch (Exception ex) { Console.WriteLine("Message :{0} ", ex.Message); }

        }

        /// <summary>
        /// Creates a private key and stores it into a file called private.key
        /// </summary>
        /// <param name="keySize">Size of the key in bits</param>
        /// <param name="DByteArr">D value as a byte array</param>
        /// <param name="d">Size of DByteArr</param>
        /// <param name="NByteArr">N value as a byte array</param>
        /// <param name="n">Size of NByteArr</param>
        public static void createPrivKey(int keySize, byte[] DByteArr, int d, byte[] NByteArr, int n) {
            // Use parameter values to create a private key represented as a byte array
            byte[] privKeyByteArr = new byte[8 + d + n];
            Array.Copy(NByteArr, 0, privKeyByteArr, 8 + d, n);
            byte[] nByteArr = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(nByteArr);
            }
            Array.Copy(nByteArr, 0, privKeyByteArr, 4 + d, 4);
            Array.Copy(DByteArr, 0, privKeyByteArr, 4, d);
            byte[] dByteArr = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(dByteArr);
            }
            Array.Copy(dByteArr, 0, privKeyByteArr, 0, 4);
            // Creates a private key object and store the string form of 
            // privKeyByteArr as the key
            PrivKeyObject privateKey = new PrivKeyObject();
            privateKey.Key = Convert.ToBase64String(privKeyByteArr);
            // Store the key in the private.key file
            try {
                using (StreamWriter outputFile = 
                    new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "private.key"))) {
                        outputFile.Write(JsonConvert.SerializeObject(privateKey));
                    }
            } catch (Exception ex) { Console.WriteLine("Message :{0} ", ex.Message); }

        }

        /// <summary>
        /// ModInverse function provided in writeup
        /// </summary>
        /// <param name="a">a value provided in writeup</param>
        /// <param name="n">n value provided in writeup</param>
        /// <returns>BigInteger to be used as D value</returns>
        static BigInteger modInverse(BigInteger a, BigInteger n) {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0) {
                BigInteger t = i / a, x = a;
                a = BigInteger.ModPow(i, 1, x); //i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v = BigInteger.ModPow(v, 1, n); //v %= n;
            if (v < 0) v = BigInteger.ModPow((v + n), 1, n); //(v + n) % n;
            return v;
        }
    }

    /// <summary>
    /// Prime generator copied over from my Project 2 work
    /// </summary>
    public static class PrimeGen {

        /// <summary>
        /// Lock object used to lock console and primesGenerated to prevent primes printing out of order.
        /// </summary>
        static object lockObject = new Object();

        /// <summary>
        /// Used to speed up process of checking if a number is prime. If a number is divisible by any of these
        /// primes, it is not a prime number.
        /// </summary>
        /// <value>The first 1000 prime numbers.</value>
        public static int[] firstThousandPrimes = new int[] {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 
        59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 
        181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 
        313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 
        457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 
        607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 
        757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 
        919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 
        1061, 1063, 1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 
        1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 
        1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 
        1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 
        1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 
        1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889, 1901, 
        1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999, 2003, 2011, 2017, 2027, 2029, 2039, 2053, 
        2063, 2069, 2081, 2083, 2087, 2089, 2099, 2111, 2113, 2129, 2131, 2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207, 
        2213, 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287, 2293, 2297, 2309, 2311, 2333, 2339, 2341, 2347, 
        2351, 2357, 2371, 2377, 2381, 2383, 2389, 2393, 2399, 2411, 2417, 2423, 2437, 2441, 2447, 2459, 2467, 2473, 2477, 
        2503, 2521, 2531, 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617, 2621, 2633, 2647, 2657, 2659, 2663, 
        2671, 2677, 2683, 2687, 2689, 2693, 2699, 2707, 2711, 2713, 2719, 2729, 2731, 2741, 2749, 2753, 2767, 2777, 2789, 
        2791, 2797, 2801, 2803, 2819, 2833, 2837, 2843, 2851, 2857, 2861, 2879, 2887, 2897, 2903, 2909, 2917, 2927, 2939, 
        2953, 2957, 2963, 2969, 2971, 2999, 3001, 3011, 3019, 3023, 3037, 3041, 3049, 3061, 3067, 3079, 3083, 3089, 3109, 
        3119, 3121, 3137, 3163, 3167, 3169, 3181, 3187, 3191, 3203, 3209, 3217, 3221, 3229, 3251, 3253, 3257, 3259, 3271, 
        3299, 3301, 3307, 3313, 3319, 3323, 3329, 3331, 3343, 3347, 3359, 3361, 3371, 3373, 3389, 3391, 3407, 3413, 3433, 
        3449, 3457, 3461, 3463, 3467, 3469, 3491, 3499, 3511, 3517, 3527, 3529, 3533, 3539, 3541, 3547, 3557, 3559, 3571, 
        3581, 3583, 3593, 3607, 3613, 3617, 3623, 3631, 3637, 3643, 3659, 3671, 3673, 3677, 3691, 3697, 3701, 3709, 3719, 
        3727, 3733, 3739, 3761, 3767, 3769, 3779, 3793, 3797, 3803, 3821, 3823, 3833, 3847, 3851, 3853, 3863, 3877, 3881, 
        3889, 3907, 3911, 3917, 3919, 3923, 3929, 3931, 3943, 3947, 3967, 3989, 4001, 4003, 4007, 4013, 4019, 4021, 4027, 
        4049, 4051, 4057, 4073, 4079, 4091, 4093, 4099, 4111, 4127, 4129, 4133, 4139, 4153, 4157, 4159, 4177, 4201, 4211, 
        4217, 4219, 4229, 4231, 4241, 4243, 4253, 4259, 4261, 4271, 4273, 4283, 4289, 4297, 4327, 4337, 4339, 4349, 4357, 
        4363, 4373, 4391, 4397, 4409, 4421, 4423, 4441, 4447, 4451, 4457, 4463, 4481, 4483, 4493, 4507, 4513, 4517, 4519, 
        4523, 4547, 4549, 4561, 4567, 4583, 4591, 4597, 4603, 4621, 4637, 4639, 4643, 4649, 4651, 4657, 4663, 4673, 4679, 
        4691, 4703, 4721, 4723, 4729, 4733, 4751, 4759, 4783, 4787, 4789, 4793, 4799, 4801, 4813, 4817, 4831, 4861, 4871, 
        4877, 4889, 4903, 4909, 4919, 4931, 4933, 4937, 4943, 4951, 4957, 4967, 4969, 4973, 4987, 4993, 4999, 5003, 5009, 
        5011, 5021, 5023, 5039, 5051, 5059, 5077, 5081, 5087, 5099, 5101, 5107, 5113, 5119, 5147, 5153, 5167, 5171, 5179, 
        5189, 5197, 5209, 5227, 5231, 5233, 5237, 5261, 5273, 5279, 5281, 5297, 5303, 5309, 5323, 5333, 5347, 5351, 5381, 
        5387, 5393, 5399, 5407, 5413, 5417, 5419, 5431, 5437, 5441, 5443, 5449, 5471, 5477, 5479, 5483, 5501, 5503, 5507, 
        5519, 5521, 5527, 5531, 5557, 5563, 5569, 5573, 5581, 5591, 5623, 5639, 5641, 5647, 5651, 5653, 5657, 5659, 5669, 
        5683, 5689, 5693, 5701, 5711, 5717, 5737, 5741, 5743, 5749, 5779, 5783, 5791, 5801, 5807, 5813, 5821, 5827, 5839, 
        5843, 5849, 5851, 5857, 5861, 5867, 5869, 5879, 5881, 5897, 5903, 5923, 5927, 5939, 5953, 5981, 5987, 6007, 6011, 
        6029, 6037, 6043, 6047, 6053, 6067, 6073, 6079, 6089, 6091, 6101, 6113, 6121, 6131, 6133, 6143, 6151, 6163, 6173, 
        6197, 6199, 6203, 6211, 6217, 6221, 6229, 6247, 6257, 6263, 6269, 6271, 6277, 6287, 6299, 6301, 6311, 6317, 6323, 
        6329, 6337, 6343, 6353, 6359, 6361, 6367, 6373, 6379, 6389, 6397, 6421, 6427, 6449, 6451, 6469, 6473, 6481, 6491, 
        6521, 6529, 6547, 6551, 6553, 6563, 6569, 6571, 6577, 6581, 6599, 6607, 6619, 6637, 6653, 6659, 6661, 6673, 6679, 
        6689, 6691, 6701, 6703, 6709, 6719, 6733, 6737, 6761, 6763, 6779, 6781, 6791, 6793, 6803, 6823, 6827, 6829, 6833, 
        6841, 6857, 6863, 6869, 6871, 6883, 6899, 6907, 6911, 6917, 6947, 6949, 6959, 6961, 6967, 6971, 6977, 6983, 6991, 
        6997, 7001, 7013, 7019, 7027, 7039, 7043, 7057, 7069, 7079, 7103, 7109, 7121, 7127, 7129, 7151, 7159, 7177, 7187, 
        7193, 7207, 7211, 7213, 7219, 7229, 7237, 7243, 7247, 7253, 7283, 7297, 7307, 7309, 7321, 7331, 7333, 7349, 7351, 
        7369, 7393, 7411, 7417, 7433, 7451, 7457, 7459, 7477, 7481, 7487, 7489, 7499, 7507, 7517, 7523, 7529, 7537, 7541, 
        7547, 7549, 7559, 7561, 7573, 7577, 7583, 7589, 7591, 7603, 7607, 7621, 7639, 7643, 7649, 7669, 7673, 7681, 7687, 
        7691, 7699, 7703, 7717, 7723, 7727, 7741, 7753, 7757, 7759, 7789, 7793, 7817, 7823, 7829, 7841, 7853, 7867, 7873, 
        7877, 7879, 7883, 7901, 7907, 7919};

        /// <summary>
        /// Uses Parallel.For to efficiently generate random numbers and check if they are prime.
        /// If prime, it is printed to output, if not it is ignored.
        /// </summary>
        /// <param name="bitsParam">Number of bits of each individual prime number.</param>
        public static BigInteger PrimeNumberGenerator(int bitsParam) {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            BigInteger num = GenerateRandomNumber(bitsParam, rng);
            while (!CheckIfPrime(num, bitsParam)) { num = GenerateRandomNumber(bitsParam, rng); }
            rng.Dispose();

            return num;
        }

        /// <summary>
        /// Checks first thousand primes and calls Miller-Rabin Primality Test to check if a number is prime.
        /// </summary>
        /// <param name="number">Number to check primality for.</param>
        /// <param name="numBits">Number of bits of the number.</param>
        /// <returns>True if prime, false if not.</returns>
        public static Boolean CheckIfPrime(BigInteger number, int numBits) {
            // First 1000 primes check. If a number is divisible by any of these primes, it is not a prime number.
            // Do this only for bit sizes of 1024 or greater, as any smaller will reduce efficiency.
            if (numBits >= 1024) {
                foreach (int prime in firstThousandPrimes) {
                    if (number % prime == 0) {
                        return false;
                    }
                }
            }

            // Call the Miller-Rabin Primality Test if conditions pass.
            // Number must be greater than 3 and odd.
            if (number > 3 && number % 2 != 0) {
                if (number.IsProbablyPrime()) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates a random number of the given bit length.
        /// </summary>
        /// <param name="bits">Bit length of the number to be generated.</param>
        /// <param name="rng">Random number generator to be reused.</param>
        /// <returns>BigInteger of specified bit length.</returns>
        public static BigInteger GenerateRandomNumber(int bits, RandomNumberGenerator rng) {
            byte[] byteArr = new byte[bits / 8];
            rng.GetBytes(byteArr);
            BigInteger generated = new BigInteger(byteArr, true, false);
            
            return generated;
        }

        /// <summary>
        /// C# Version of the Miller-Rabin Primality Test.
        /// </summary>
        /// <param name="value">Odd BigInteger to be tested for primality.</param>
        /// <param name="k">Number of rounds of testing to perform.</param>
        /// <returns>False if value is composite, true otherwise.</returns>
        static Boolean IsProbablyPrime(this BigInteger value, int k = 10) {
            BigInteger d = value - 1;
            int r = 0;
            while (d % 2 == 0) {
                d /= 2;
                r++;
            }

            Random rand = new Random();
            for (int i = 0; i < k; i++) {
                // Pick random number between 2 and value - 2.
                BigInteger a = BigInteger.Remainder(new BigInteger(rand.Next()), value - 3) + 2;
                if (a < 2) { a += 2; }

                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1) { continue; }

                for (int j = 0; j < r - 1; j++) {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == value - 1) { break; }
                }
                if (x == value - 1) { continue; }
                return false;
            }
            return true;
        }
    }
}