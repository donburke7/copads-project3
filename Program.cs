
using System.Numerics;
using Newtonsoft.Json;

namespace Messenger {
    public class Program {

        public static void Main(String[] args) {
            if (args.Length > 1 && args[1] is not null) {
                var keyClient = new KeyClient(args[1]);
                var msgClient = new MsgClient(args[1]);
                if (args[0].Equals("keyGen")) {
                    if (Int32.TryParse(args[1], out int keySize) && keySize > 0) {
                        KeyGenerator keyGen = new KeyGenerator(keySize);
                        keyGen.genKeys();
                    }
                    else { Console.Out.WriteLine(args[1] + " must be a positive int."); }
                }
                else if (args[0].Equals("getKey")) {
                    keyClient.getKey();
                }
                else if (args[0].Equals("sendKey")) {
                    keyClient.sendKey();
                }
                else if (args[0].Equals("getMsg")) {
                    msgClient.getMsg();
                }
                else if (args[0].Equals("sendMsg")) {
                    msgClient.sendMsg(args[2]);
                }

            }
            
        }

        
    }
}