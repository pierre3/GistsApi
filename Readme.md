# Gists API C# library

This is [Gists API v3](http://developer.github.com/v3/gists/) library for .Net.
- Target: .Net Framework4.5
- HttpClient ([System.Net.Http Namespace](http://msdn.microsoft.com/library/system.net.http.aspx)) based async methods.
- Parsing JSON using [DynamicJson](http://dynamicjson.codeplex.com/).

## Install
from nuget gallery  
- https://nuget.org/packages/GistsApi/

```
PM> Install-Package GistApi
```

## GistClient class

```cs
public class GistClient
{
    public async Task Authorize(string authCode);

    //POST https://api.github.com/gists
    public async Task<GistObject> CreateAGist(string description, bool isPublic, IEnumerable<Tuple<string, string>> fileContentCollection);
    
    //PATCH https://api.github.com/gists/:id
    public async Task<GistObject> EditAGist(string id, string description, string targetFilename, string content);

    //PATCH https://api.github.com/gists/:id (Change filename)
    public async Task<GistObject> EditAGist(string id, string description, string oldFilename, string newFilename, string content);
    
    //PATCH https://api.github.com/gists/:id (Set "null" to filename)
    public async Task<GistObject> DeleteAFile(string id, string description, string filename);
    
    //DELETE https://api.github.com/gists/:id
    public async Task DeleteAGist(string id);
    
    //GET https://api.github.com/gists (List authenticated users gist)
    public async Task<IEnumerable<GistObject>> ListGists();

    //GET https://api.github.com/users/:user/gists
    public async Task<IEnumerable<GistObject>> ListGists(string user);

    //GET https://api.github.com/users/:user/gists?since=:date_time
    public async Task<IEnumerable<GistObject>> ListGists(string user, DateTime since);
    
    //List PublicGists, UsersGists, AuthenticatedUserGists or AuthenticatedUserStarredGists
    public async Task<IEnumerable<GistObject>> ListGists(ListMode mode);
    
    //GET https://api.github.com/gists/:id
    public async Task<GistObject> GetSingleGist(string id);
    
    //POST https://api.github.com/gists/:id/forks
    public async Task<GistObject> ForkAGist(string id);
    
    //PUT https://api.github.com/gists/:id/star
    public async Task StarAGist(string id);
    
    //DELETE https://api.github.com/gists/:id/star
    public async Task UnstarAGist(string id);

    //GET row_url (from API response)
    public async Task<string> DownloadRawText(Uri rawUrl);
    
    //Cancel all pending requests
    public void Cancel();
    
    //List mode for ListGists method 
    public enum ListMode
    {
        PublicGists,
        UsersGists,
        AuthenticatedUserGists,
        AuthenticatedUserStarredGists
    }
}

```
## Usage
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
    
    gistClient = new GistClient("clientID", "clientSecret", "userAgent");
    
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

### Exsamples

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

        //Download content text of "MyGist_File1".
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

See __WpfGists__ source code for details.
- [WpfGists.ViewModel.GistsWindowViewModel.cs](https://github.com/pierre3/GistsApi/blob/master/WpfGists.ViewModel/GistsWindowViewModel.cs)

## WPFGists
GistsAPI client GUI for Windows. 
![sample window](https://raw.github.com/pierre3/Images/master/GistApiSampleWindow.png)

To run this application, specify "clientID" and "clientSecret".

1. Register application, and get your "clientID" and "clientSecret".
 =>[Register a new OAuth application](https://github.com/settings/applications/new)
2. Edit a "WpfGists.exe.config" file.

```xml
<applicationSettings>
    <WpfGists.Properties.Settings>
        <setting name="ClientID" serializeAs="String">
            <value>your clientID</value>
        </setting>
        <setting name="ClientSecret" serializeAs="String">
            <value>your clientSecret</value>
        </setting>
    </WpfGists.Properties.Settings>
</applicationSettings>
```

## License 
[Microsoft Public License (MS-PL)](http://opensource.org/licenses/MS-PL)
