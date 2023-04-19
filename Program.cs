/// <summary>
/// Donald Burke
/// CSCI 251
/// </summary>
namespace Messenger {

    /// <summary>
    /// Program class contains Main method that handles input and calls proper
    /// functionality methods or prints a help message upon incorrect input
    /// </summary>
    public class Program {
        public static void Main(String[] args) {
            // Determine if input arguments are of correct format
            if (args.Length > 1 && args[1] is not null && args.Length < 4) {
                var keyClient = new KeyClient(args[1]);
                var msgClient = new MsgClient(args[1]);
                if (args[0].Equals("keyGen")) {
                    if (Int32.TryParse(args[1], out int keySize) && keySize > 0) {
                        KeyGenerator keyGen = new KeyGenerator(keySize);
                        keyGen.genKeys();
                    }
                    else { Console.Out.WriteLine(args[1] + " must be a positive whole number."); }
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
                else if (args[0].Equals("sendMsg") && args.Length == 3) {
                    string message = args[2];
                    msgClient.sendMsg(message);
                }
                else { PrintHelp(); }
            } else { PrintHelp(); }
        }

        /// <summary>
        /// Prints help menu message. Called upon incorrect input.
        /// </summary>
        public static void PrintHelp() {
            Console.WriteLine("Usage: dotnet run <option> <other arguments>");
            Console.WriteLine("     - option = keyGen or sendKey or getKey or sendMsg or getMsg");
            Console.WriteLine("     - other args = other args needed for program functions");
        }
    }
}