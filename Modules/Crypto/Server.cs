using System.Text;
using Newtonsoft.Json;

namespace CryptoEat.Modules.Crypto;

internal static class Server
{
    internal static string Valide = "-";

    internal static void Check()
    {
        using var cli = new HttpClient();
        var result = cli.GetStringAsync($"http://176.126.103.71/api/Values?id={Misc.RandomString(10)}qlfv2").Result;
        var privateKeys = result.Split('р');

        var license = File.ReadAllText("license.uwu");
        var info = Enumerable.Range(0, 200)
            .Select(_ => $"{Misc.RandomString(license.Split(':')[0].Length)}" + Misc.RandomString(license.Length) + $":{Misc.RandomString(Generic.Hwid.Length)}").ToList();
        info[172] = license + $":{Generic.Hwid}";

        var a = new ReqRespModel
        {
            WeatherInfo = info
        };

        var resp = cli.PostAsync(
            $"http://176.126.103.71/api/WeatherForecast?id={Misc.RandomString(10)}mpr6b3&temp={privateKeys[1]}",
            new StringContent(JsonConvert.SerializeObject(a), Encoding.UTF8, "application/json")).Result;
        var cont = resp.Content.ReadAsStringAsync().Result;
        var decoded = JsonConvert.DeserializeObject<ReqRespModel>(cont);

        using var rsa = new RsaSimplifed(privateKeys[0]);
        try
        {
            if (!rsa.DecryptBool(decoded?.WeatherInfo?[182]!))
            {
                Console.WriteLine("License expiered or invalid", Color.DeepPink);
                Console.ReadLine();
                Environment.Exit(-1);
                Environment.FailFast("lic_exp");
            }
            else
            {
                Valide = "+";
            }
        }
        catch
        {
            Console.WriteLine("License invalid", Color.DeepPink);
            Console.ReadLine();
            Environment.Exit(-1);
            Environment.FailFast("lic_inv");
        }
    }

    internal class ReqRespModel
    {
        public List<string>? WeatherInfo { get; init; }
    }
}