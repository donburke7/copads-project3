using System.Numerics;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Donald Burke
/// CSCI 251
/// </summary>
namespace Messenger {

    /// <summary>
    /// Represents a client that is used to get and send public keys, as well as
    /// to process public and private keys for encryption and decryption of messages
    /// </summary>
    public class KeyClient {
        
        /// <summary>
        /// Email given through user input
        /// </summary>
        private string email;

        /// <summary>
        /// E value for a public key
        /// </summary>
        public BigInteger E;

        /// <summary>
        /// D value for a private key
        /// </summary>
        public BigInteger D;

        /// <summary>
        /// N value used by both public and private keys
        /// </summary>
        public BigInteger N;

        /// <summary>
        /// KeyClient constructor
        /// </summary>
        /// <param name="email">Email provided through user input</param>
        public KeyClient(string email) {
            this.email = email;
        }

        /// <summary>
        /// Retrieves a public key from the server for a given email
        /// </summary>
        /// <returns>void</returns>
        public async void getKey() {
            using (var client = new HttpClient()) {
                try {
                    // HTTP GET request to server
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Key/");
                    var response = client.GetAsync(email).Result;
                    // Checks for 200 OK response before continuing to process output
                    if (response.IsSuccessStatusCode) {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (responseBody == string.Empty) {
                            Console.WriteLine("Could not find key for " + email);
                        }
                        else {
                            try {
                                // Writes public key retrieved to a file of name "email".key
                                // to allow for multiple keys to be stored
                                using (StreamWriter outputFile = 
                                    new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), email + ".key"))) {
                                        outputFile.Write(responseBody);
                                    }
                            } catch (Exception e) { Console.WriteLine("Message :{0} ", e.Message); }
                        }
                    } else { Console.WriteLine("Status Code: " + response.StatusCode); }
                } catch (HttpRequestException e) {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
        }

        /// <summary>
        /// Uploads a public key to the server to allow for users to send messages to said email
        /// </summary>
        public void sendKey() {
            using (var client = new HttpClient()) {
                try {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Key/");
                    // Read public key from file and check if it has content
                    string jsonPubKey = File.ReadAllText("public.key");
                    PubKeyObject? pubKeyObj = JsonConvert.DeserializeObject<PubKeyObject>(jsonPubKey);
                    if (pubKeyObj is not null && pubKeyObj.Key is not null) {
                        pubKeyObj.Email = email;
                        // Create json including email and key and do HTTP PUT to server
                        jsonPubKey = JsonConvert.SerializeObject(pubKeyObj);
                        var content = new StringContent(jsonPubKey, Encoding.UTF8, "application/json");
                        var response = client.PutAsync(email, content).Result;
                        // Check if 200 OK and output status message based on result
                        if (response.IsSuccessStatusCode) {
                            Console.WriteLine("Key saved");
                        } else { Console.WriteLine("Error, status code: " + response.StatusCode); }
                    } else { Console.WriteLine("Public key not created."); }

                    // Write email to own private key
                    var jsonPrivKey = File.ReadAllText("private.key");
                    PrivKeyObject? privKeyObj = JsonConvert.DeserializeObject<PrivKeyObject>(jsonPrivKey);
                    if (privKeyObj is not null) {
                        if (privKeyObj.Email is not null) {
                            if (!privKeyObj.Email.Contains(email)) {
                                privKeyObj.Email.Add(email);
                            }
                        } 
                        else { 
                            privKeyObj.Email = new List<string>();
                            privKeyObj.Email.Add(email);
                        }
                        jsonPrivKey = JsonConvert.SerializeObject(privKeyObj);
                        using (StreamWriter outputFile = 
                                    new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "private.key"))) {
                                        outputFile.Write(jsonPrivKey);
                                    }
                    }
                } catch (Exception e) {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message: {0} ", e.Message);
                }
            }
        }

        /// <summary>
        /// Processes my private key and assigns values to D and N for object to be used
        /// in message decryption
        /// </summary>
        public void processPrivKey() {
            // Gets an person's private key and gathers D and N values for message decryption
            var jsonKey = File.ReadAllText("private.key");
            PrivKeyObject? keyObj = JsonConvert.DeserializeObject<PrivKeyObject>(jsonKey);
            if (keyObj is not null && keyObj.Key is not null) {
                // Create byte array from keyObject key   
                byte[] byteArr = Convert.FromBase64String(keyObj.Key);
                
                // Create int d
                byte[] dArr = new byte[4];
                Array.Copy(byteArr, 0, dArr, 0, 4);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(dArr);
                }
                int d = BitConverter.ToInt32(dArr);

                // Create BigInt D
                byte[] DArr = new byte[d];
                Array.Copy(byteArr, 4, DArr, 0, d);
                this.D = new BigInteger(DArr);

                // Create int n
                byte[] nArr = new byte[4];
                Array.Copy(byteArr, 4 + d, nArr, 0, 4);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(nArr);
                }
                int n = BitConverter.ToInt32(nArr);

                // Create BigInt N
                byte[] NArr = new byte[n];
                Array.Copy(byteArr, 8 + d, NArr, 0, n);
                this.N = new BigInteger(NArr);

            }
        }

        /// <summary>
        /// Processes a public key and assigns values to E and N for object to be used
        /// in message encryption
        /// </summary>
        public void processPubKey() {
            // Gets an person's public key and gathers E and N values for message encryption
            var jsonKey = File.ReadAllText(email + ".key");
            PubKeyObject? keyObj = JsonConvert.DeserializeObject<PubKeyObject>(jsonKey);
            if (keyObj is not null && keyObj.Key is not null) {
                // Create byte array from keyObject key   
                byte[] byteArr = Convert.FromBase64String(keyObj.Key);
                
                // Create int e
                byte[] eArr = new byte[4];
                Array.Copy(byteArr, 0, eArr, 0, 4);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(eArr);
                }
                int e = BitConverter.ToInt32(eArr);

                // Create BigInt E
                byte[] EArr = new byte[e];
                Array.Copy(byteArr, 4, EArr, 0, e);
                this.E = new BigInteger(EArr);

                // Create int n
                byte[] nArr = new byte[4];
                Array.Copy(byteArr, 4 + e, nArr, 0, 4);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(nArr);
                }
                int n = BitConverter.ToInt32(nArr);

                // Create BigInt N
                byte[] NArr = new byte[n];
                Array.Copy(byteArr, 8 + e, NArr, 0, n);
                this.N = new BigInteger(NArr);
            }
        }

    }

    /// <summary>
    /// Object to represent a private key
    /// </summary>
    public class PrivKeyObject {
        /// <summary>
        /// Emails of public keys that have been sent out
        /// </summary>
        /// <value>User inputted email during sendKey()</value>
        [JsonProperty("email")]
        public List<string>? Email { get; set; }

        /// <summary>
        /// Private key storage
        /// </summary>
        /// <value>My private key</value>
        [JsonProperty("key")]
        public string? Key { get; set; }
    }

    /// <summary>
    /// Object to represent a public key
    /// </summary>
    public class PubKeyObject {
        /// <summary>
        /// The email related to the public key
        /// </summary>
        /// <value></value>
        [JsonProperty("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Storage for the public key
        /// </summary>
        /// <value>The public key</value>
        [JsonProperty("key")]
        public string? Key { get; set; }
    }
}