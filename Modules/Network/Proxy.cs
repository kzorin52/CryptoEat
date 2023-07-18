using System.Net;
using System.Text.RegularExpressions;
using Leaf.xNet;

namespace CryptoEat.Modules.Network;

internal partial class Proxy
{
    private static readonly Regex Regex = ProxyLoginPass();
    private static readonly Regex Regex2 = ProxyDefault();
    private WebProxy? _proxy;
    internal string? Ip;
    internal string? Login, Password, Format, Raw;
    internal int Port;
    private HttpRequest? _request;

    internal bool IsSystem = false;

    internal static Proxy? InitFromString(string str, string format)
    {
        if (Regex.IsMatch(str))
        {
            var values = Regex.Match(str).Groups.Values.ToArray();
            var proxy = new Proxy
            {
                Ip = values[1].Value,
                Port = int.Parse(values[2].Value),
                Login = values[3].Value,
                Password = values[4].Value,
                Format = format,
                Raw = str,
                _request = new HttpRequest
                {
                    Proxy = ProxyClient.Parse(format switch
                    {
                        "socks4" => ProxyType.Socks4,
                        "socks5" => ProxyType.Socks5,
                        "socks4a" => ProxyType.Socks4A,
                        _ => ProxyType.HTTP
                    }, str),
                    ConnectTimeout = 5000,
                    KeepAlive = false
                }
            };
            return proxy;
        }

        if (!Regex2.IsMatch(str)) return null;
        {
            var values = Regex2.Match(str).Groups.Values.ToArray();
            var proxy = new Proxy
            {
                Ip = values[1].Value,
                Port = int.Parse(values[2].Value),
                Format = format,
                Raw = str,
                _request = new HttpRequest
                {
                    Proxy = ProxyClient.Parse(format switch
                    {
                        "socks4" => ProxyType.Socks4,
                        "socks5" => ProxyType.Socks5,
                        "socks4a" => ProxyType.Socks4A,
                        _ => ProxyType.HTTP
                    }, str),
                    ConnectTimeout = 7000
                }
            };
            return proxy;
        }
    }

    internal WebProxy GetWebProxy()
    {
        if (Login == null && Password == null)
            _proxy ??= new WebProxy
            {
                Address = new Uri($"{Format}://{Ip}:{Port}"),
                UseDefaultCredentials = false
            };

        return _proxy ??= new WebProxy
        {
            Address = new Uri($"{Format}://{Ip}:{Port}"),
            Credentials = new NetworkCredential(Login, Password),
            UseDefaultCredentials = false
        };
    }

    internal HttpRequest GetHttpRequest()
    {
        return _request ??= new HttpRequest
        {
            Proxy = ProxyClient.Parse(Format switch
            {
                "socks4" => ProxyType.Socks4,
                "socks5" => ProxyType.Socks5,
                "socks4a" => ProxyType.Socks4A,
                _ => ProxyType.HTTP
            }, Raw),
            ConnectTimeout = 7000
        };
    }

    internal bool Check(bool title = true)
    {
        try
        {
            _request ??= GetHttpRequest();

            if (title)
            {
                Helpers.Count++;
                Helpers.SetTitle("Checking proxies...");
            }

            var response = _request.Get("https://debank.com/");
            return response.IsOK;
        }
        catch (HttpException)
        {
            return true;
        }
        catch (ProxyException)
        {
            if (Format != "socks4") return false;

            _request ??= GetHttpRequest();

            Format = "socks4a";
            _request.Proxy = ProxyClient.Parse(ProxyType.Socks4A, Raw);
            return Check(false);
        }
        catch
        {
            return false;
        }
    }

    [GeneratedRegex("(.+):(.+):(.+):(.+)", RegexOptions.Compiled)]
    private static partial Regex ProxyLoginPass();

    [GeneratedRegex("(.+):(.+)", RegexOptions.Compiled)]
    private static partial Regex ProxyDefault();
}