using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GistsApi
{
  public class GistClient
  {
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _scope;
    private string _accessToken;
    private CancellationTokenSource cancellationTS;

    public Uri AuthorizeUrl
    {
      get
      {
        return new Uri(string.Format("https://github.com/login/oauth/authorize?client_id={0}&scope={1}",
          _clientId, _scope));
      }
    }

    public GistClient(string clientKey, string clientSecret)
    {
      this._clientId = clientKey;
      this._clientSecret = clientSecret;
      this.cancellationTS = new CancellationTokenSource();
      this._scope = "gist";
    }

    public void Cancel()
    {
      cancellationTS.Cancel();
    }

    public async Task Authorize(string authCode)
    {
      //POST https://github.com/login/oauth/access_token
      //Accept: application/json
      //{"access_token":"e72e16c7e42f292c6912e7710c838347ae178b4a","token_type":"bearer"}

      var requestUrl = new Uri(string.Format("https://github.com/login/oauth/access_token?client_id={0}&client_secret={1}&code={2}",
        _clientId, _clientSecret, authCode));
      using (var httpClient = CreateHttpClient())
      {
        var response = await httpClient.PostAsync(requestUrl, null, cancellationTS.Token);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        var json = DynamicJson.Parse(responseString);
        _accessToken = (string)json.access_token;
      }
    }

    public async Task<IEnumerable<GistObject>> ListGists()
    {
      //GET /gists
      var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", _accessToken));
      using (var httpClient = CreateHttpClient())
      {
        var response = await httpClient.GetAsync(requestUrl, cancellationTS.Token);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        var json = (dynamic[])DynamicJson.Parse(responseString);
        return json.Select(j => (GistObject)DynamicToGistObject(j));
      }
    }

    public async Task<GistObject> GetSingleGist(string id)
    {
      //GET /gists/:id
      var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
      using (var httpClient = CreateHttpClient())
      {
        var response = await httpClient.GetAsync(requestUrl, cancellationTS.Token);
        var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task<GistObject> CreateAGist(string description, bool isPublic, IEnumerable<Tuple<string, string>> fileContentCollection)
    {
      //POST /gists
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", _accessToken));
        var content = MakeCreateContent(description, isPublic, fileContentCollection);
        var data = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUrl, data, cancellationTS.Token);
        var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task<GistObject> EditAGist(string id, string description, string targetFilename, string content)
    {
      //PATCH /gists/:id
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
        var httpMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl);
        var editData = MakeEditContent(description, targetFilename, content);
        httpMessage.Content = new StringContent(editData, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(httpMessage, cancellationTS.Token);
        var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task<GistObject> EditAGist(string id, string description, string oldFilename, string newFilename, string content)
    {
      //PATCH /gists/:id
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
        var httpMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl);
        var editData = MakeEditContent(description, oldFilename, newFilename, content);
        httpMessage.Content = new StringContent(editData, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(httpMessage, cancellationTS.Token);
        var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task<GistObject> DeleteAFile(string id, string description, string filename)
    {
      //PATCH /gists/:id
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
        var httpMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl);
        var editData = MakeDeleteFileContent(description, filename);
        httpMessage.Content = new StringContent(editData, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(httpMessage, cancellationTS.Token);
        var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task DeleteAGist(string id)
    {
      //DELETE /gists/:id
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
        var response = await httpClient.DeleteAsync(requestUrl);
        response.EnsureSuccessStatusCode();
      }
    }

    public async Task<string> DownloadRawText(Uri rawUrl)
    {
      using (var httpClient = CreateHttpClient())
      {
        var response = await httpClient.GetAsync(rawUrl, cancellationTS.Token);
        return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
      }
    }

    protected HttpClient CreateHttpClient()
    {
      if (cancellationTS.IsCancellationRequested)
      { cancellationTS = new CancellationTokenSource(); }

      var httpClient = new HttpClient();
      var accept = System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json");
      httpClient.DefaultRequestHeaders.Accept.Add(accept);
      return httpClient;
    }

    protected static string MakeCreateContent(string _description, bool _isPublic, IEnumerable<Tuple<string, string>> fileContentCollection)
    {
      dynamic _result = new DynamicJson();
      dynamic _file = new DynamicJson();
      _result.description = _description;
      _result.@public = _isPublic.ToString().ToLower();
      _result.files = new { };
      foreach (var fileContent in fileContentCollection)
      {
        _result.files[fileContent.Item1] = new { filename = fileContent.Item1, content = fileContent.Item2 };
      }
      return _result.ToString();
    }

    protected static string MakeEditContent(string _description, string _targetFileName, string _content)
    {
      dynamic _result = new DynamicJson();
      dynamic _file = new DynamicJson();
      _result.description = _description;
      _result.files = new { };
      _result.files[_targetFileName] = new { content = _content };
      return _result.ToString();
    }

    protected static string MakeEditContent(string _description, string _oldFileName, string _newFileName, string _content)
    {
      dynamic _result = new DynamicJson();
      dynamic _file = new DynamicJson();
      _result.description = _description;
      _result.files = new { };
      _result.files[_oldFileName] = new { filename = _newFileName, content = _content };
      return _result.ToString();
    }

    protected static string MakeDeleteFileContent(string _description, string filename)
    {
      dynamic _result = new DynamicJson();
      dynamic _file = new DynamicJson();
      _result.description = _description;
      _result.files = new { };
      _result.files[filename] = "null";
      return _result.ToString();
    }

    protected static GistObject DynamicToGistObject(dynamic json)
    {
      var gist = (GistObject)json;
      var files = ((DynamicJson)json.files).DeserializeMembers(member =>
        new File()
        {
          filename = member.filename,
          raw_url = member.raw_url,
          size = member.size
        });

      gist.files = new Files(files.ToArray());
      return gist;
    }
  }
}
