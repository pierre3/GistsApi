# Gists API C# library

- .Net Framework4.5
- HttpClient (System.Net.Http Namespace) 
- [DynamicJson](http://dynamicjson.codeplex.com/)

## GistClient class

```cs
public class GistClient
{
    public Uri AuthorizeUrl{get;}
    public GistClient(string clientKey, string clientSecret);
    
    public async Task<GistObject> CreateAGist(string description, string fileName, bool isPublic, string content);
    public async Task<IEnumerable<GistObject>> ListGists();
    public async Task<GistObject> EditAGist(string id,string description, string filename, string content);
    public async Task DeleteAGist(string id);
    public async Task<string> DownloadRawText(Uri rawUrl); 
    
    public void Cancel();
}

//Returns Object 
public class GistObject
{
    public string url { set; get; }
    public string id { set; get; }
    public string description { set; get; }
    public bool @public { set; get; }
    public User user { set; get; }
    public Files files { set; get; }
    public double comments { set; get; }
    public string comments_url { set; get; }
    public string html_url { set; get; }
    public string git_pull_url { set; get; }
    public string git_push_url { set; get; }
    public string created_at { set; get; }
    public Fork[] forks { get; set; }
    public History[] history { get; set; }
}
```
## OAuth 2.0 flow (for WPF)
xaml
```xaml
<WebBrowser Name="webBrowser" LoadCompleted="webBrowser_LoadCompleted"/>
```
xaml.cs
```cs
//Constructor
public MainWindow()
{
    InitializeComponent();
    
    gistClient = new GistClient("clientID", "clientSecret");
    
    //navigate to "https://github.com/login/oauth/authorize" 
    webBrowser.Navigate(gistClient.AuthorizeUrl);
}

private async void webBrowser_LoadCompleted(object sender, NavigationEventArgs e) 
{
    if (e.Uri == null)
    { return; }

    if (e.Uri.AbsoluteUri.Contains("code="))
    {
        var authCode = Regex.Split(result.Uri.AbsoluteUri, "code=")[1];
        
        //get access token
        await gistClient.Authorize(authCode)
   }
}
```

## Exsamples

```cs
try
{
    ShowMessageMethod("List gists...");
    
    var myGist = await gistClient.ListGists()
        .First(gist=>gist.Files.First().filename == "MyGist.md");
    var downloadText = await gistClient.DownloadRawText(myGist.Files.First().raw_url);
    
    ShowMessageMethod("Completed.");
    ShowMessageMethod(downloadText);
}
catch (System.Net.Http.HttpRequestException e)
{
    ShowMessageMethod("Error." + e.Message);
}
catch (OperationCanceledException)
{
    ShowMessageMethod("Canceled".);
}

```
