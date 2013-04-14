# Gists API C# library

This is [Gists API](http://developer.github.com/v3/gists/) library for .Net.
- Target: .Net Framework4.5
- HttpClient ([System.Net.Http Namespace](http://msdn.microsoft.com/library/system.net.http.aspx)) based REST Interface.
- Parsing JSON using [DynamicJson](http://dynamicjson.codeplex.com/).

## GistClient class

```cs
public class GistClient
{
    public Uri AuthorizeUrl{get;}
    
    public GistClient(string clientKey, string clientSecret);
    
    public async Task Authorize(string authCode);
    public async Task<IEnumerable<GistObject>> ListGists();
    public async Task<GistObject> GetSingleGist(string id);
    
    public async Task<GistObject> CreateAGist(string description, bool isPublic, IEnumerable<Tuple<string, string>> fileContentCollection);
    
    public async Task<GistObject> EditAGist(string id, string description, string targetFilename, string content);
    public async Task<GistObject> EditAGist(string id, string description, string oldFilename, string newFilename, string content);
    public async Task<GistObject> DeleteAFile(string id, string description, string filename);
    
    public async Task DeleteAGist(string id);
    public async Task<string> DownloadRawText(Uri rawUrl);
    
    public void Cancel();
}

```
## Exsamples
### OAuth 2.0 flow (for WPF)
xaml
```xml
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

### Usage

```cs

private GistClient gistClient;

public async void ListGists()
{
    try
    {
    
        ShowMessage("List gists...");

        var gists = await gistClient.ListGists();
    
        var myGist = gists.First(gist => gist.files.Any(file => file.filename == "MyGist_File1"));
        var rawUrl = myGist.files.First(file => file.filename == "MyGist_File1").raw_url;
    
        var downloadText = await gistClient.DownloadRawText(rawUrl);

        ShowMessage("Completed.");
        ShowMessage(downloadText);
    }

    catch (System.Net.Http.HttpRequestException e)
    {
        ShowMessageMethod("Error." + e.Message);
    }
    catch (OperationCanceledException)
    {
        ShowMessageMethod("Canceled".);
    }
}

public async void CreateAGist()
{
    try
    {
        var description = "gist description";
        bool isPublic = true;
        var uploadFiles = new[]{
            Tuple.Create("file1.txt", "file content..."),
            Tuple.Create("file2.cs", "using system; ..."),
            Tuple.Create("file2.md", "# Readme Gists API ...")
        };
        
       ShowMessage("Create a gist")
       
       await gistClient.CreateAGist(description,isPublic,uploadFiles);
       
       ShowMessage("Completed.");
       
    } 
    catch (System.Net.Http.HttpRequestException e)
    {
        ShowMessage = "Error. " + e.Message;
    }
    catch (OperationCanceledException)
    {
        ShowMessage = "Canceled.";
    }
}

//
// Cancel pending requests.
//
public void Cancel()
{
    //Throw OperationCanceledException
    gistClient.Cancel();
}

```

## WPF Sample Project
![sample window](https://raw.github.com/pierre3/Images/master/GistApiSampleWindow.png)

To run this application,command-line argument specifies "clientID" and "clientSecret".  
Register application, and get your "clientID" and "clientSecret".
 =>[Register a new OAuth application](https://github.com/settings/applications/new)


- See WpfGists source code for details.
  + [WpfGists.ViewModel.GistsWindowViewModel.cs](https://github.com/pierre3/GistsApi/blob/master/WpfGists.ViewModel/GistsWindowViewModel.cs)
