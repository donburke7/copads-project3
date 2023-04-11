using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GetKeyApplication 
{

    public class GetKey {
        public static readonly HttpClient client;

        public static void Main(String[] args) {
            string testEmail = "toivcs@rit.edu";

            var KeyGet = new GetKey();
            KeyGet.getKeyClient(testEmail);
        }

        public async void getKeyClient(string email) {
            string responseBody = await client.GetStringAsync("http://kayrun.cs.rit.edu:5000/Message/" + email);

            Console.WriteLine(responseBody);
        }
    }
}