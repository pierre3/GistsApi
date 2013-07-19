using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace GistsApi
{
    public class GistClient
    {
        #region Fields
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _scope;
        private string _accessToken;
        private string _userAgent = "";
        private CancellationTokenSource cancellationTS;
        private HttpResponseHeaders _responseHeaders;
        #endregion

        #region Properties
        public string FirstLinkUrl { get; protected set; }
        public string LastLinkUrl { get; protected set; }
        public string NextLinkUrl { get; protected set; }
        public string PrevLinkUrl { get; protected set; }
        public Uri AuthorizeUrl
        {
            get
            {
                return new Uri(string.Format("https://github.com/login/oauth/authorize?client_id={0}&scope={1}",
                    this._clientId, this._scope));
            }
        }
        #endregion

        #region Constructors
        public GistClient(string clientKey, string clientSecret, string userAgent)
        {
            this._clientId = clientKey;
            this._clientSecret = clientSecret;
            this.cancellationTS = new CancellationTokenSource();
            this._scope = "gist";
            this._userAgent = userAgent;
        }
        #endregion

        #region Methods
        public async Task Authorize(string authCode)
        {
            var requestUri = new Uri(string.Format("https://github.com/login/oauth/access_token?client_id={0}&client_secret={1}&code={2}", this._clientId, this._clientSecret, authCode));
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var response = await httpClient.PostAsync(requestUri, null, this.cancellationTS.Token);
                string responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                object json = DynamicJson.Parse(responseString);
                this._accessToken = (string)((dynamic)json).access_token;
            }
        }

        public void Cancel()
        {
            this.cancellationTS.Cancel();
        }

        public async Task<GistObject> CreateAGist(string description, bool isPublic, IEnumerable<Tuple<string, string>> fileContentCollection)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists?access_token={0}", this._accessToken));
                
                string content = MakeCreateContent(description, isPublic, fileContentCollection);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync(requestUri, data, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                
                string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
            }
        }

        protected HttpClient CreateHttpClient()
        {
            if (this.cancellationTS.IsCancellationRequested)
            {
                this.cancellationTS = new CancellationTokenSource();
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(this._userAgent);
            return client;
        }

        public async Task<GistObject> DeleteAFile(string id, string description, string filename)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, this._accessToken));
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
               
                string content = MakeDeleteFileContent(description, filename);
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                
                var response = await httpClient.SendAsync(request, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                
                string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
            }
        }

        public async Task DeleteAGist(string id)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, this._accessToken));
                var response = await httpClient.DeleteAsync(requestUri);
                this._responseHeaders = response.Headers;
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<string> DownloadRawText(Uri rawUrl)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var response = await httpClient.GetAsync(rawUrl, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            }
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

        public async Task<GistObject> EditAGist(string id, string description, string targetFilename, string content)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, this._accessToken));
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
                
                string editData = MakeEditContent(description, targetFilename, content);
                request.Content = new StringContent(editData, Encoding.UTF8, "application/json");
                
                var response = await httpClient.SendAsync(request, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                
                string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
            }
        }

        public async Task<GistObject> EditAGist(string id, string description, string oldFilename, string newFilename, string content)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, this._accessToken));
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
                
                var editData = MakeEditContent(description, oldFilename, newFilename, content);
                request.Content = new StringContent(editData, Encoding.UTF8, "application/json");
                
                var response = await httpClient.SendAsync(request, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                
                string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
            }
        }

        public async Task<GistObject> ForkAGist(string id)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/forks?access_token={1}", id, this._accessToken));
                
                var response = await httpClient.PostAsync(requestUri, null, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                
                string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
            }
        }

        public async Task<GistObject> GetSingleGist(string id)
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, this._accessToken));
            var json = await this.GetStringAsync(requestUrl);
            return (GistObject)DynamicToGistObject((dynamic)DynamicJson.Parse(json));
        }

        public async Task<IEnumerable<GistObject>> ListGists()
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", this._accessToken));
            return await this.ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(ListMode mode)
        {
            Uri requestUrl;
            if (mode == ListMode.PublicGists)
            {
                requestUrl = new Uri("https://api.github.com/gists/public");
            }
            else
            {
                requestUrl = new Uri(string.Format("https://api.github.com/gists{0}?access_token={1}", 
                    (mode == ListMode.AuthenticatedUserStarredGists) ? "/starred" : "", this._accessToken));
            }
            return await this.ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(string user)
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/users/{0}/gists", user));
            return await this.ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(Uri requestUrl)
        {
            //GET /gists
            using (var httpClient = CreateHttpClient())
            {
                var response = await GetStringAsync(requestUrl);
                SetLinkUrl();
                var json = (dynamic[])DynamicJson.Parse(response);
                return json.Select(j => (GistObject)DynamicToGistObject(j));
            }
        }

        public async Task<IEnumerable<GistObject>> ListGists(string user, DateTime since)
        {
            Uri requestUrl = new Uri(string.Format("https://api.github.com/users/{0}/gists?since={1}", user, since.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")));
            return await this.ListGists(requestUrl);
        }
        
        public async Task StarAGist(string id)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/star?access_token={1}", id, this._accessToken));
                var response = await httpClient.PutAsync(requestUri, null);
                this._responseHeaders = response.Headers;
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task UnstarAGist(string id)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/star?access_token={1}", id, this._accessToken));
                var response = await httpClient.DeleteAsync(requestUri);
                this._responseHeaders = response.Headers;
                response.EnsureSuccessStatusCode();
            }
        }

        protected async Task<string> GetStringAsync(Uri requestUrl)
        {
            using (HttpClient httpClient = this.CreateHttpClient())
            {
                var response = await httpClient.GetAsync(requestUrl, this.cancellationTS.Token);
                this._responseHeaders = response.Headers;
                return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            }
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

        protected void SetLinkUrl()
        {
            this.FirstLinkUrl = "";
            this.PrevLinkUrl = "";
            this.NextLinkUrl = "";
            this.LastLinkUrl = "";
            var pair = this._responseHeaders.FirstOrDefault(h => h.Key == "Link");
            if (pair.Key != "Link")
            { return; }
            string linkValue = pair.Value.FirstOrDefault<string>();
            if (linkValue == null)
            { return; }

            foreach (string item in linkValue.Split(new char[] { ',' }).Select(s => s.Trim()))
            {
                var token = item.Split(new char[] { ';' });
                if (token.Length < 2)
                { continue; }

                var url = token[0].Trim().TrimStart(new char[] { '<' }).TrimEnd(new char[] { '>' });
                switch (token[1].Trim())
                {
                    case "rel=\"first\"":
                        this.FirstLinkUrl = url;
                        break;

                    case "rel=\"prev\"":
                        this.PrevLinkUrl = url;
                        break;

                    case "rel=\"next\"":
                        this.NextLinkUrl = url;
                        break;
                    case "rel=\"last\"":
                        this.LastLinkUrl = url;
                        break;
                }
            }
        }

        #endregion

        #region Nested Classes
        public enum ListMode
        {
            PublicGists,
            UsersGists,
            AuthenticatedUserGists,
            AuthenticatedUserStarredGists
        }
        #endregion
    }
}
