using System.Numerics;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Donald Burke
/// CSCI 251
/// </summary>
namespace Messenger {
    
    /// <summary>
    /// Represents a client that is used to get and send messages
    /// </summary>
    public class MsgClient {

        /// <summary>
        /// Email to be used for messaging task
        /// </summary>
        public string email;

        /// <summary>
        /// MsgClient constructor
        /// </summary>
        /// <param name="email">User inputted email</param>
        public MsgClient(string email) {
            this.email = email;
        }
        
        /// <summary>
        /// Sends an encrytped message to this client's email
        /// </summary>
        /// <param name="message">Message to be encrypted and sent</param>
        public void sendMsg(string message) {
            try {
                // Ensures that public key exists for the email
                string pubJson = File.ReadAllText(email + ".key");
                // Encrypts message using encryption algorithm and turns it back into a string
                byte[] msgByteArr = Encoding.UTF8.GetBytes(message);
                BigInteger msgBigInt = new BigInteger(msgByteArr);
                KeyClient keyProcessor = new KeyClient(email);
                keyProcessor.processPubKey();
                BigInteger cipherBigInt = BigInteger.ModPow(msgBigInt, keyProcessor.E, keyProcessor.N);
                byte[] cipherByteArr = cipherBigInt.ToByteArray();
                string cipherOutput = Convert.ToBase64String(cipherByteArr);

                using (var client = new HttpClient()) {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Message/");
                    // Creates a message object and stores the recipient email and content in it
                    Message? messageObj = new Message();
                    messageObj.Email = email;
                    messageObj.Content = cipherOutput;
                    // Turns it into a json
                    string messageJson = JsonConvert.SerializeObject(messageObj);
                    var content = new StringContent(messageJson, Encoding.UTF8, "application/json");
                    // Performs an HTTP PUT for that content
                    var response = client.PutAsync(email, content).Result;
                    // Checks for 200 OK response status and prints it based on results
                    if (response.IsSuccessStatusCode) {
                        Console.WriteLine("Message written");
                    } else { Console.WriteLine("Error, status code: " + response.StatusCode); }
                } 
            } catch (FileNotFoundException) { Console.WriteLine("Public key not found for: " + email); }
        }

        /// <summary>
        /// Gets an encrypted message from an emails "inbox"
        /// </summary>
        /// <returns>void</returns>
        public async void getMsg() {
            try {
                // Creates a private key object from the private key file
                string privJson = File.ReadAllText("private.key");
                PrivKeyObject? privKey = JsonConvert.DeserializeObject<PrivKeyObject>(privJson);
                if (privKey != null && privKey.Email != null && privKey.Email.Contains(email)) {
                    using (var client = new HttpClient()) {
                        try {
                            // Performs an HTTP GET to the server for a message
                            client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Message/");
                            var response = client.GetAsync(email).Result;
                            // Checks for 200 OK status code
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
                                    // Turn decrypted byte array into string and output to console
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

    /// <summary>
    /// Object representing a message that is sent or received
    /// </summary>
    public class Message {
        /// <summary>
        /// Email the message is from or going to
        /// </summary>
        /// <value>User inputted email</value>
        [JsonProperty("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        /// <value>User inputted message</value>
        [JsonProperty("content")]
        public string? Content { get; set; }
    }
}