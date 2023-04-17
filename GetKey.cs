

using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messenger {
    public class KeyClient {
        private string email;
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
                        // JObject parsedJson = JObject.Parse(responseBody);
                        // JToken? key = parsedJson["key"];
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
                    var jsonPubKey = File.ReadAllText("public.key");
                    KeyObject? pubKeyObj = JsonConvert.DeserializeObject<KeyObject>(jsonPubKey);
                    if (pubKeyObj is not null && pubKeyObj.Key is not null) {
                        pubKeyObj.Email = email;
                        jsonPubKey = JsonConvert.SerializeObject(pubKeyObj);
                        var content = new StringContent(jsonPubKey, Encoding.UTF8, "application/json");
                        var response = client.PutAsync(email, content).Result;
                        if (response.IsSuccessStatusCode) {
                            Console.WriteLine("Key saved");
                        } else { response.EnsureSuccessStatusCode(); }
                    } else { Console.WriteLine("Public key not created."); }
                } catch (Exception e) {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message: {0} ", e.Message);
                }
            }
        }

        public void processKey() {
            // Gets an person's public key and gathers E and N values for message encryption
            var jsonKey = File.ReadAllText($"{email}.key");
            KeyObject? keyObj = JsonConvert.DeserializeObject<KeyObject>(jsonKey);
            if (keyObj is not null && keyObj.Key is not null) {
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

    public class KeyObject {

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("key")]
        public string? Key { get; set; }
    }
}