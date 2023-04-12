
namespace Messenger {
    public class Program {

        public static void Main(String[] args) {
            string testEmail = "toivcs@rit.edu";

            var KeyGet = new GetKeyClient();
            KeyGet.getKey(testEmail);

        }

        
    }
}