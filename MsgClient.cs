

using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace Messenger {


    public class MsgClient {
        public string email;

        public MsgClient(string email) {
            this.email = email;
        }
        
        public void sendMsg(string message) {
            try {
                string pubJson = File.ReadAllText(email + ".key");
                byte[] msgByteArr = Convert.FromBase64String(message);
                BigInteger msgBigInt = new BigInteger(msgByteArr);
                KeyClient keyProcessor = new KeyClient(email);
                keyProcessor.processPubKey();
                BigInteger cipherBigInt = BigInteger.ModPow(msgBigInt, keyProcessor.E, keyProcessor.N);
                byte[] cipherByteArr = cipherBigInt.ToByteArray();
                string cipherOutput = Convert.ToBase64String(cipherByteArr);

                using (var client = new HttpClient()) {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Message/");

                    Message? messageObj = new Message();
                    messageObj.Email = email;
                    messageObj.Content = cipherOutput;
                    string messageJson = JsonConvert.SerializeObject(messageObj);
                    var content = new StringContent(messageJson, Encoding.UTF8, "application/json");
                    
                    var response = client.PutAsync(email, content).Result;
                    if (response.IsSuccessStatusCode) {
                        Console.WriteLine("Message written");
                    } else { Console.WriteLine("Error, status code: " + response.StatusCode); }
                } 
            } catch (FileNotFoundException) { Console.WriteLine("Public key not found: " + email); }
        }

        public async void getMsg() {
            try {
                string privJson = File.ReadAllText("private.key");
                PrivKeyObject? privKey = JsonConvert.DeserializeObject<PrivKeyObject>(privJson);

                if (privKey != null && privKey.Email != null && privKey.Email.Contains(email)) {
                    using (var client = new HttpClient()) {
                        try {
                            client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Message/");
                            var response = client.GetAsync(email).Result;
                            if (response.IsSuccessStatusCode) {
                                string responseBody = await response.Content.ReadAsStringAsync();
                                
                                // Gather encrypted message from response
                                Message? message = JsonConvert.DeserializeObject<Message>(responseBody);
                                if (message is not null && message.Content is not null) {
                                    // Begin message decryption process
                                    byte[] cipherByteArr = Convert.FromBase64String(message.Content);
                                    BigInteger cipherBigInt = new BigInteger(cipherByteArr);
                                    KeyClient keyProcessor = new KeyClient(email);
                                    keyProcessor.processPrivKey();
                                    BigInteger messageBigInt = BigInteger.ModPow(cipherBigInt, keyProcessor.D, keyProcessor.N);
                                    byte[] messageByteArr = messageBigInt.ToByteArray();
                                    string messageOutput = Encoding.UTF8.GetString(messageByteArr);
                                    Console.WriteLine(messageOutput);
                                } else { Console.WriteLine("No messages found for " + email); }
                            } else { Console.WriteLine("Error, status code: " + response.StatusCode); }
                        } catch (Exception e) { Console.WriteLine(e); }
                    }
                } else { Console.WriteLine("No private key found for: " + email); }

            } catch (Exception e) { Console.WriteLine(e); }
        }
    }

    public class Message {
        [JsonProperty("email")]
        public string? Email;

        [JsonProperty("content")]
        public string? Content;
    }
}