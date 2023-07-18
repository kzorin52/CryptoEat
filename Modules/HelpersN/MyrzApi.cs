using Newtonsoft.Json;

namespace CryptoEat.Modules.HelpersN;

public class MyrzApi : IDisposable
{
    #region Fields

    protected readonly HttpClient Client = new();
    protected string Endpoint;
    protected string Key;

    #endregion

    #region Constructor

    public MyrzApi(string key, string endpoint = "http://antipublic.one/api/")
    {
        Key = key;
        Endpoint = endpoint;
        Client.BaseAddress = new Uri(endpoint);
        Client.Timeout = new TimeSpan(0, 0, 30);
    }

    #endregion

    #region Methods

    private static async Task<T> ParseJsonResp<T>(HttpResponseMessage resp) =>
        JsonConvert.DeserializeObject<T>(await resp.Content.ReadAsStringAsync())!;

    private async Task<T> RequestPost<T>(string path, Dictionary<string, string> requestParams) =>
        await ParseJsonResp<T>(await Client.PostAsync(path, new FormUrlEncodedContent(requestParams)));

    private async Task<T> RequestGet<T>(string path, Dictionary<string, string> requestParams)
    {
        var str1 = await new FormUrlEncodedContent(requestParams).ReadAsStringAsync();
        var str2 = !path.Contains('?') ? "?" + str1 : "&" + str1;
        return JsonConvert.DeserializeObject<T>(await Client.GetStringAsync(path + str2))!;
    }

    public async Task<CheckLinesRespLine[]> CheckLines(
        string[] lines,
        bool insert) =>
        await ParseJsonResp<CheckLinesRespLine[]>(await Client.PostAsync("part_search.php?key=" + Key,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {
                    "type",
                    insert ? nameof(insert) : "noinsert"
                },
                {
                    nameof(lines),
                    string.Join(" ", lines)
                }
            })));

    public async Task<AntiPublicAccess> CheckAccess()
    {
        var api = this;
        var publicAccessResp = await api.RequestGet<AntiPublicAccessResp>("check.php?key=" + api.Key,
            new Dictionary<string, string>
            {
                {
                    "plus",
                    "1"
                }
            });
        return new AntiPublicAccess
        {
            general = publicAccessResp.message == "Access is exists",
            plus = publicAccessResp.plus == "Access is exists"
        };
    }

    public async Task<EmailSearchResult> SearchEmail(string email)
    {
        var api = this;
        var emailSearchResp = await api.RequestPost<EmailSearchResp>("email_search.php?key=" + api.Key,
            new Dictionary<string, string>
            {
                {
                    nameof(email),
                    email
                }
            });
        if (emailSearchResp.error != null)
        {
            throw new ApiException(emailSearchResp.error);
        }

        var strArray = new string[emailSearchResp.results.Length];
        for (var index = 0; index < emailSearchResp.results.Length; ++index)
        {
            strArray[index] = emailSearchResp.results[index].line;
        }

        return new EmailSearchResult
        {
            success = emailSearchResp.success,
            lines = strArray,
            availableQueries = emailSearchResp.awailableQueries
        };
    }

    public async Task<int> GetAvailableQueries()
    {
        var api = this;
        var availableQueriesResp = await api.RequestPost<AvailableQueriesResp>("email_search.php?key=" + api.Key,
            new Dictionary<string, string>
            {
                {
                    "do",
                    "access"
                }
            });
        return availableQueriesResp.error == null
            ? availableQueriesResp.availableQueries
            : throw new ApiException(availableQueriesResp.error);
    }

    public async Task<long> GetLineCount() =>
        await RequestGet<long>("count_lines.php?key=" + Key, new Dictionary<string, string>());

    public async Task<EmailPasswordsResponse> GetEmailPasswords(
        IEnumerable<string> emails,
        int passwordLimit)
    {
        var jsonResp = await ParseJsonResp<EmailPasswordsResponse>(await Client.PostAsync(
            "email_part_search.php?key=" + Key, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {
                    "lines",
                    string.Join(" ", emails)
                },
                {
                    "limit",
                    passwordLimit.ToString()
                }
            })));
        return string.IsNullOrEmpty(jsonResp.error) ? jsonResp : throw new ApiException(jsonResp.error);
    }

    #endregion

    #region Exceptions

    private class ApiException : Exception
    {
        public ApiException(string message)
            : base(message)
        {
        }
    }

    #endregion

    #region Models

    public struct CheckLinesRespLine
    {
        public string line;
        public bool is_private;
    }

    private struct AntiPublicAccessResp
    {
        public string message;
        public string error;
        public string plus;
    }

    public struct AntiPublicAccess
    {
        public bool general;
        public bool plus;
    }

    private struct EmailSearchResp
    {
        public string error;
        public bool success;
        public EmailSearchLine[] results;
        public int awailableQueries;
        public int resultCount;
    }

    private struct EmailSearchLine
    {
        public string line;
    }

    public struct EmailSearchResult
    {
        public bool success;
        public string[] lines;
        public int availableQueries;
    }

    private struct AvailableQueriesResp
    {
        public string error;
        public int availableQueries;
    }

    public struct CheckUpdatesResult
    {
        public string url;
        public string version;
        public string changelog;
        public string archiveUrl;
    }

    public struct EmailPasswordsResponse
    {
        public string[] results;
        public int availableQueries;
        public string error;
    }

    #endregion

    public void Dispose() => Client.Dispose();
}