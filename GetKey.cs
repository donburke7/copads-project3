using Newtonsoft.Json.Linq;


namespace Messenger {
    public class GetKeyClient {
        public async void getKey(string email) {

            using (var client = new HttpClient()) {
                try {
                    client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000/Key/");
                    var response = client.GetAsync(email).Result;
                    if (response.IsSuccessStatusCode) {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        // JObject parsedJson = JObject.Parse(responseBody);
                        // JToken? key = parsedJson["key"];
                        try {
                            using (StreamWriter outputFile = 
                                new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), email + ".key"))) {
                                    outputFile.Write(responseBody);
                                }
                        } catch (Exception e) { Console.WriteLine("Message :{0} ", e.Message); }

                    } else { Console.WriteLine("Status Code: " + response.StatusCode); }

                } catch (HttpRequestException e) {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
        }
    }
}