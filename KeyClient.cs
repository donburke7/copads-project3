
using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace Messenger {
    public class KeyClient {
        private string email;
        public BigInteger E;
        public BigInteger D;
        public BigInteger N;
        public KeyClient(string email) {
            this.email = email;
        }
        public async void getKey() {
            using (var client = new HttpClient()) {
                try {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Key/");
                    var response = client.GetAsync(email).Result;
                    if (response.IsSuccessStatusCode) {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (responseBody == string.Empty) {
                            Console.WriteLine("Could not find key for " + email);
                        }
                        else {
                            try {
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

        public void sendKey() {
            using (var client = new HttpClient()) {
                try {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Key/");
                    // Send public key to server
                    string jsonPubKey = File.ReadAllText("public.key");
                    PubKeyObject? pubKeyObj = JsonConvert.DeserializeObject<PubKeyObject>(jsonPubKey);
                    if (pubKeyObj is not null && pubKeyObj.Key is not null) {
                        pubKeyObj.Email = email;
                        jsonPubKey = JsonConvert.SerializeObject(pubKeyObj);
                        var content = new StringContent(jsonPubKey, Encoding.UTF8, "application/json");
                        var response = client.PutAsync(email, content).Result;
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

        public void processPrivKey() {
            // Gets an person's public key and gathers E and N values for message encryption
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

        public void processPubKey() {
            // Gets an person's public key and gathers E and N values for message encryption
            var jsonKey = File.ReadAllText(email + ".key");
            PrivKeyObject? keyObj = JsonConvert.DeserializeObject<PrivKeyObject>(jsonKey);
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

    public class PrivKeyObject {

        [JsonProperty("email")]
        public List<string>? Email { get; set; }

        [JsonProperty("key")]
        public string? Key { get; set; }
    }

    public class PubKeyObject {
        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("key")]
        public string? Key { get; set; }
    }
}