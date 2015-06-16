using System;
using System.Net;
using Chomikuj.Rest;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace Chomikuj
{
    public sealed class ChomikujClient : ChomikujBase
    {
        private readonly ChomikujDirectory _userHomeDirectory;        

        public ChomikujDirectory HomeDirectory { get { return _userHomeDirectory; } }

        public ChomikujClient(string login, string password)
        {
            var loginRequest = GetLoginRequest(login, password);
            var result = RestClient.Post(loginRequest);

            if(result.StatusCode != HttpStatusCode.OK)
                throw new Exception("Login failed");

            var html = new HtmlDocument();
            html.LoadHtml(result.Content);

            var chomikId = GetChomikId(html);
            var folderId = GetFolderId(html);
            var responseUri = result.ResponseUri;

            _userHomeDirectory = BuildChomikujDirectory(responseUri, chomikId, folderId);
        }

        private ChomikujDirectory BuildChomikujDirectory(Uri responseUri, string chomikId, string folderId)
        {
            return new ChomikujDirectory(this)
            {
                Link = responseUri.LocalPath,
                Title = responseUri.LocalPath.Remove(0, 1),
                ChomikId = chomikId,
                FolderId = folderId
            };
        }

        private string GetFolderId(HtmlDocument html)
        {
            return html.DocumentNode.QuerySelector("input[name=\"FolderId\"]").Attributes["value"].Value;            
        }

        private string GetChomikId(HtmlDocument html)
        {
            return html.DocumentNode.QuerySelector("input[name=\"ChomikId\"]").Attributes["value"].Value;
        }

        private Request GetLoginRequest(string login, string password)
        {
            var loginRequest = new Request(LoginUrl);
            loginRequest.AddParameter("Login", login);
            loginRequest.AddParameter("Password", password);
            return loginRequest;
        }
    }
}
