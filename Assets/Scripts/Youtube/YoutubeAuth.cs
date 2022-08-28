using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Apis.Auth;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Flows;
using System.Threading.Tasks;
using System.IO;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Services;
using System.Web;
using Google.Apis.Util;
using Google.Apis.Auth.OAuth2.Responses;
using System.Net.Http;

public static class YoutubeAuth
{
    private static string clientSecret = "{\"installed\":{\"client_id\":\"1011550511606-i0gb1pvsfpesnh79q117gdbd7lab2mpg.apps.googleusercontent.com\",\"project_id\":\"mp3playerv3\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_secret\":\"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\",\"redirect_uris\":[\"http://localhost\"]}}";
    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static SimpleHTTPServer sv;
    private const int HTTP_PORT = 5259;

    private class CompatibleCodeReciever : ICodeReceiver
    {
        public string RedirectUri { get; set; }

        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    private static async Task OnAuthCodeRecieved(string code)
    {
        try
        {
            using var stream = GenerateStreamFromString(clientSecret);
            var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

            var tokenReq = new AuthorizationCodeTokenRequest();
            tokenReq.ClientId = secrets.ClientId;
            tokenReq.ClientSecret = secrets.ClientSecret;
            tokenReq.Code = code;
            tokenReq.RedirectUri = GoogleAuthConsts.LocalhostRedirectUri + ":" + HTTP_PORT.ToString();
            var resp = await tokenReq.ExecuteAsync(new System.Net.Http.HttpClient(), GoogleAuthConsts.TokenUrl, CancellationToken.None, Google.Apis.Util.SystemClock.Default);
            //UserCredential cred = new UserCredential(new AuthorizationCodeFlow(new AuthorizationCodeFlow.Initializer(GoogleAuthConsts.AuthorizationUrl, GoogleAuthConsts.TokenUrl)), "user", resp);
            UserCredential credential = new UserCredential()
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "mp3playerv3"
            });

            var req = youtubeService.Playlists.List("snippet");
            req.Mine = true;
            req.MaxResults = 50;
            var response = await req.ExecuteAsync();

            foreach (var item in response.Items)
            {
                Debug.Log(item.Snippet.Title);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public static async Task Run()
    {
        //using (var stream = GenerateStreamFromString(clientSecret))
        //{
        //    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        GoogleClientSecrets.FromStream(stream).Secrets,
        //        new[] { YouTubeService.Scope.Youtube },
        //        "user",
        //        CancellationToken.None,
        //        new FileDataStore(Utils.DataPath + typeof(YoutubeAuth).Name, true)
        //    );
        //}

        using var stream = GenerateStreamFromString(clientSecret);
        var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

        var codeRequest = new AuthorizationCodeRequestUrl(new System.Uri(GoogleAuthConsts.AuthorizationUrl));
        codeRequest.ClientId = secrets.ClientId;
        codeRequest.State = "authState";
        codeRequest.Scope = YouTubeService.Scope.Youtube;
        codeRequest.RedirectUri = GoogleAuthConsts.LocalhostRedirectUri + ":" + HTTP_PORT.ToString();

        if (sv != null) sv.Stop();
        sv = new SimpleHTTPServer(Utils.RootPath, HTTP_PORT, 16);
        sv.OnJsonSerialized += (object obj) =>
        {
            var namedParams = (Dictionary<string, object>)obj;
            _  = OnAuthCodeRecieved(namedParams["code"].ToString());
            return "Yey!!!";
        };

        Application.OpenURL(codeRequest.Build().ToString());

        //var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //{
        //    HttpClientInitializer = credential,
        //    ApplicationName = "mp3playerv3"
        //});

        //var req = youtubeService.Playlists.List("snippet");
        //req.Mine = true;
        //req.MaxResults = 50;
        //var response = await req.ExecuteAsync();

        //foreach (var item in response.Items)
        //{
        //    Debug.Log(item.Snippet.Title);
        //}
    }
}
