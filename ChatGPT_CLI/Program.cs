using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChatGPT_cli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Program program = new Program();
                await program.Run();

                Console.WriteLine("Do you want to try again? (Y/N)");
                string tryAgain = Console.ReadLine();
                if (tryAgain.ToUpper() != "Y")
                    break;
            }
        }
        
        private async Task Run()
        {
            Console.WriteLine("\u001b[32mChatGPT: \u001b[0m" + " Welcome to ChatGPT");
            Console.WriteLine("\u001b[32mChatGPT: \u001b[0m" + " How can I help you today?");

            string apiKeyFilePath = Path.Combine(Environment.CurrentDirectory, "api_key.txt");

            if (!IsApiKeyExists(apiKeyFilePath))
            {
                Console.WriteLine("But, firstly enter your API key:");
                string apiKey = Console.ReadLine();
                if (!AddApiKey(apiKey, apiKeyFilePath))
                {
                    return;
                }
            }

            Console.WriteLine(string.Empty);

            while (true)
            {
                Console.Write("\u001b[31mYou: \u001b[0m");
                var prompt = Console.ReadLine();
                var response = await SendPrompt(prompt, apiKeyFilePath);
                response = response.Replace("\\n", Environment.NewLine);
                Console.WriteLine(string.Empty);
                Console.WriteLine("\u001b[32mChatGPT: \u001b[0m" + response);
            }
        }

        bool IsApiKeyExists(string apiKeyFilePath)
        {
            try
            {
                return File.Exists(apiKeyFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        bool AddApiKey(string apiKey, string apiKeyFilePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    File.WriteAllText(apiKeyFilePath, apiKey);
                    Console.WriteLine("API key added.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid API key format.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public async Task<string> SendPrompt(string message, string apiKeyFilePath)
        {
            try
            {
                string apiKey = File.ReadAllText(apiKeyFilePath);

                var httpClient = new HttpClient();
                var url = "https://api.openai.com/v1/chat/completions";
                var requestJson = $@"{{
                    ""model"": ""gpt-3.5-turbo"",
                    ""messages"": [
                    {{
                        ""role"": ""system"",
                        ""content"": ""Advanced programmer""
                    }},
                    {{
                        ""role"": ""user"",
                        ""content"": ""{message}""
                    }}
                    ]
                }}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("Authorization", "Bearer " + apiKey);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    JObject responseContentJson = JObject.Parse(responseContent);
                    var choices = responseContentJson["choices"];
                    
                    return choices[0]["message"]["content"].ToString();

                }
                else
                {
                    return "Error: " + response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

    }
}
