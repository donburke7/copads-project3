
using System.Numerics;
using Newtonsoft.Json;

namespace Messenger {
    public class Program {

        public static void Main(String[] args) {
            string testEmail = "toivcs@rit.edu";

            var KeyGet = new GetKeyClient();
            KeyGet.getKey(testEmail);

            // Gets an person's public key and gathers E and N values for message encryption
            var jsonKey = File.ReadAllText($"{testEmail}.key");
            KeyObject? keyObj = JsonConvert.DeserializeObject<KeyObject>(jsonKey);
            if (keyObj is not null) {
                // Create byte array from keyObject key   
                byte[] byteArr = Convert.FromBase64String(keyObj.Key);
                
                // Create int e
                byte[] eArr = new byte[4];
                Array.Copy(byteArr, 0, eArr, 0, 4);
                Array.Reverse(eArr);
                int e = BitConverter.ToInt32(eArr, 0);

                // Create BigInt E
                byte[] EArr = new byte[e];
                Array.Copy(byteArr, 4, EArr, 0, e);
                Array.Reverse(EArr);
                BigInteger E = new BigInteger(EArr);

                // Create int n
                byte[] nArr = new byte[4];
                Array.Copy(byteArr, 4 + e, nArr, 0, 4);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(nArr);
                }
                int n = BitConverter.ToInt32(nArr, 0);

                // Create BigInt N
                byte[] NArr = new byte[e];
                Array.Copy(byteArr, 8 + e, NArr, 0, n);
                Array.Reverse(NArr);
                BigInteger N = new BigInteger(NArr);


            }
            

        }

        
    }
}