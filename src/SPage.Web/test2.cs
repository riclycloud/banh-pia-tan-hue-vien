using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestClient {
    public static async Task Run() {
        var handler = new HttpClientHandler { UseCookies = true, AllowAutoRedirect = false };
        var client = new HttpClient(handler);
        var getRes = await client.GetAsync("http://localhost:5275/account/login");
        var html = await getRes.Content.ReadAsStringAsync();
        var match = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]*)\"");
        var token = match.Groups[1].Value;

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", ""),
            new KeyValuePair<string, string>("Password", ""),
            new KeyValuePair<string, string>("RememberMe", "false"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        });

        // POST to /account/login
        var postRes = await client.PostAsync("http://localhost:5275/account/login", content);
        Console.WriteLine("Status: " + (int)postRes.StatusCode);
        foreach (var h in postRes.Headers) {
            Console.WriteLine(h.Key + ": " + string.Join(", ", h.Value));
        }
    }
}
