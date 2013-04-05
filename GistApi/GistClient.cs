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
      get {
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
      cancellationTS.Dispose();
      cancellationTS = new CancellationTokenSource();
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
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var json = DynamicJson.Parse(responseString);
        _accessToken = (string)json.access_token;
      }
    }

    public async Task<IEnumerable<GistObject>> ListGists()
    {
      //GET /gists
      
      if (string.IsNullOrEmpty(_accessToken))
      { throw new InvalidOperationException("AccessToken is empty."); }

      var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", _accessToken));
      using (var httpClient = CreateHttpClient())
      {
        var response = await httpClient.GetAsync(requestUrl,cancellationTS.Token);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var json = (dynamic[])DynamicJson.Parse(responseString);
        return json.Select(j=>(GistObject)DynamicToGistObject(j));
      }
    }

    public async Task<GistObject> CreateAGist(string description, string fileName, bool isPublic, string content)
    {
      if (string.IsNullOrEmpty(_accessToken))
      { throw new InvalidOperationException("AccessToken is empty."); }

      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", _accessToken));
        
        var uploadData = MakeUploadData(description, fileName, isPublic, content);
        var bytes = System.Text.Encoding.UTF8.GetBytes(uploadData);
        var data = new ByteArrayContent(bytes);

        var response = await httpClient.PostAsync(requestUrl, data, cancellationTS.Token);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task<GistObject> EditAGist(string id,string description, string filename, string content)
    {
      //PATCH /gists/:id
      if (string.IsNullOrEmpty(_accessToken))
      { throw new InvalidOperationException("AccessToken is empty."); }
      
      using (var httpClient = CreateHttpClient())
      {
        var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
        var editData = MakeUploadData(description, filename, content);
        var bytes = System.Text.Encoding.UTF8.GetBytes(editData);
        
        var httpMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl);
        httpMessage.Content =new ByteArrayContent(bytes);
        
        var response = await httpClient.SendAsync(httpMessage, cancellationTS.Token);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return DynamicToGistObject(DynamicJson.Parse(json));
      }
    }

    public async Task DeleteAGist(string id)
    { 
      //DELETE /gists/:id
      if (string.IsNullOrEmpty(_accessToken))
      { throw new InvalidOperationException("AccessToken is empty."); }

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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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

    protected string MakeUploadData(string _description, string _filename, bool _isPublic, string _content)
    {
      var result = DynamicJson.Serialize(new
      {
        description = _description,
        @public = _isPublic.ToString().ToLower(),
        files = new { _x_FILE_NAME_x_ = new { filename = _filename, content = _content } }
      });
      return result.Replace("_x_FILE_NAME_x_", _filename);
    }

    protected string MakeUploadData(string _description, string _filename, string _content)
    {
      var result = DynamicJson.Serialize(new
      {
        description = _description,
        files = new { _x_FILE_NAME_x_ = new { content = _content } }
      });
      return result.Replace("_x_FILE_NAME_x_", _filename);
    }

    protected GistObject DynamicToGistObject(dynamic json)
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
