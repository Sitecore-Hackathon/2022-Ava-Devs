using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mvp.Feature.Forms.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using static Mvp.Feature.Forms.Constants;
using Mvp.Feature.Forms.Shared.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Mvp.Feature.Forms.Controllers
{
    public class MenteeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApplicationController> _logger;
        public MenteeController(IConfiguration configuration, ILogger<ApplicationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUserEmailClaim()
        {
            if (!User.Identity.IsAuthenticated)
                return Json(string.Empty);

            //First get user claims    
            var claims = User.Claims.ToList();
            //Filter specific claim    
            var claim = claims?.FirstOrDefault(x => x.Type.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(claim))
                return Json(string.Empty);

            return Json(claim);
        }

        private static void AddOktaAuthHeaders(WebRequest request, HttpContext httpContext)
        {
            var str = GetAuthenticationHeader(httpContext);

            if (string.IsNullOrEmpty(str))
                return;

            if (request.Headers.AllKeys.Contains("authorization"))
                return;

            request.Headers.Add("authorization", "Bearer " + str);
        }

        private static string GetAuthenticationHeader(HttpContext context)
        {
            //This simply gets the same token that the app uses, you can use MSAL to get a new token specifically for this "API" call
            try
            {
                return context.GetTokenAsync("id_token").Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting token: " + e);
                return string.Empty;
            }
        }
        [HttpGet]
        public IActionResult GetMenteeLists()
        {

            // Create a request using a URL that can receive a post.
            var sitecoreCdUri = _configuration.GetValue<string>("Sitecore:InstanceUri");
            WebRequest request = WebRequest.Create($"{sitecoreCdUri}/api/sitecore/Mentee/GetMenteeLists");

            request.Method = "GET";

            // Get the response.
            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            var responseFromServer = string.Empty;
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
            }

            // Close the response.
            response.Close();
            return Json(responseFromServer);

        }

        [HttpGet]
        public IActionResult GetMenteeInfo()
        {
            ApplicationInfo applicationInfo = GetMentee();
            if (applicationInfo != null)
            {
                if (applicationInfo.Status == ApplicationStatus.NotLoggedIn)
                {
                    return Json(new { IsLoggedIn = false });
                }
                else if (applicationInfo.Status == ApplicationStatus.PersonItemNotFound)
                {
                    return Json(new { IsLoggedIn = true, ApplicationAvailable = false });
                }
                else if (applicationInfo.Status == ApplicationStatus.ApplicationItemNotFound)
                {
                    return Json(new { IsLoggedIn = true, ApplicationAvailable = false });
                }
                else if (applicationInfo.Status == ApplicationStatus.ApplicationFound)
                {
                    return Json(new { IsLoggedIn = true, ApplicationAvailable = true, Result = applicationInfo });
                }
                else if (applicationInfo.Status == ApplicationStatus.ApplicationCompleted)
                {
                    return Json(new { IsLoggedIn = true, ApplicationCompleted = true });
                }

            }

            return Json(new { IsLoggedIn = false, ApplicationAvailable = false });

        }

        private List<Person> GetMentorList()
        {
            // Create a request using a URL that can receive a post.
            var sitecoreUri = Environment.GetEnvironmentVariable("Application_CMS_URL");

            WebRequest request = WebRequest.Create($"{sitecoreUri}/api/sitecore/Mentee/GetMentorLists");
            request.Method = "GET";
            request.ContentType = "application/json";
            WebResponse response = request.GetResponse();

            var responseFromServer = string.Empty;
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
            }

            // Close the response.
            response.Close();
            List<Person> mentorList = JsonConvert.DeserializeObject<List<Person>>(responseFromServer);
            return mentorList;
        }

        private ApplicationInfo GetMentee()
        {
            // Create a request using a URL that can receive a post.
            var sitecoreUri = Environment.GetEnvironmentVariable("Application_CMS_URL");

            WebRequest request = WebRequest.Create($"{sitecoreUri}/api/sitecore/Mentee/GetMenteeInfo");

            var user = HttpContext.User;
            var identity = (ClaimsIdentity)user?.Identity;
            string oktaId = identity?.FindFirst(_configuration.GetValue<string>("Claims:OktaId"))?.Value;
            var email = identity?.FindFirst(_configuration.GetValue<string>("Claims:Email"))?.Value;

            AddOktaAuthHeaders(request, HttpContext);

            // Set the Method property of the request to POST.
            request.Method = "POST";
            request.ContentType = "application/json";

            string requestData = JsonConvert.SerializeObject(new
            {
                identifier = oktaId,
                email = email
            });

            var data = new UTF8Encoding().GetBytes(requestData);

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
            }

            // Get the response.
            WebResponse response = request.GetResponse();

            var responseFromServer = string.Empty;
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
            }

            // Close the response.
            response.Close();
            ApplicationInfo applicationInfo = JsonConvert.DeserializeObject<ApplicationInfo>(responseFromServer);
            return applicationInfo;
        }




        private string CreateMentee(string category, string countryResidence, string techSkill, string firstName, string lastName, string email)
        {
            var createdItemId = "";
            var sitecoreUri = Environment.GetEnvironmentVariable("Application_CMS_URL");
            var itemName = ItemUtil.ProposeValidItemName(firstName + " " + lastName);
            var createPerson = new CreatePerson
            {
                ItemName = itemName,
                TemplateID = _configuration.GetValue<string>("Sitecore:PersonTemplateId"),
                FirstName = firstName,
                LastName = lastName,
                Country= countryResidence,
                Category= category,
                TechSkills= techSkill,
                Email= email
            };

            var createItemUrl = $"{sitecoreUri}{SSCAPIs.ItemApi}{$"sitecore%2Fcontent%2FMvpSite%2FMVP%20Repository%2FPeople?database=master"}";
            var request = (HttpWebRequest)WebRequest.Create(createItemUrl);

            request.Method = "POST";
            request.ContentType = "application/json";
            
            var requestBody = JsonConvert.SerializeObject(createPerson);

            var data = new UTF8Encoding().GetBytes(requestBody);

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
            }

            var response = request.GetResponse();

            _logger.LogDebug($"Item Status:\n\r{((HttpWebResponse)response).StatusDescription}");

            //Item was created - store item ID in sesion and respond
            if (((HttpWebResponse)response).StatusCode == HttpStatusCode.Created)
            {
                createdItemId = response.Headers["Location"].Substring(response.Headers["Location"].LastIndexOf("/"), response.Headers["Location"].Length - response.Headers["Location"].LastIndexOf("/")).Split("?")[0].TrimStart('/');
                string itemPath = GetItemPath(createdItemId);
                return createdItemId + "||" + itemPath;
            }

            return createdItemId;
        }

        [HttpPost]
        public IActionResult SubmitForm(string category, string countryResidence, string techSkills, string firstName, string lastName,string email)
        {
            var menteeId = CreateMentee(category, countryResidence, techSkills, firstName, lastName,email );
            var result = ExecuteRematch(category, countryResidence, techSkills, firstName, lastName, email);
            return Json(new { success = true, responseText = "Rematch succesffuly done.", list = result });
        }

        private List<Person> ExecuteRematch(string category, string countryResidence, string techSkills, string firstName, string lastName, string email)
        {
            var mentorList = GetMentorList();

            return mentorList.Where(x => x.CategoryId.Replace("{", "").Replace("}","").ToLower() == category.ToLower() && x.CountryId.Replace("{", "").Replace("}", "").ToLower() == countryResidence.ToLower()).ToList();
            
        }

        

        private string GetItemPath(string itemId)
        {
            var sitecoreUri = Environment.GetEnvironmentVariable("Application_CMS_URL");
            var updateItemByPathUrl = $"{sitecoreUri}{SSCAPIs.ItemApi}{itemId.Trim('{').Trim('}')}/?database=master&language=en&fields=ItemPath";

            var cookies = Authenticate();
            var request = (HttpWebRequest)WebRequest.Create(updateItemByPathUrl);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Cookie", cookies);

            var response = request.GetResponse();

            _logger.LogDebug($"Item Status:\n\r{((HttpWebResponse)response).StatusDescription}");

            if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
            {
                var responseFromServer = string.Empty;
                using (var dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseFromServer = reader.ReadToEnd();
                }
                GetPerson results = JsonConvert.DeserializeObject<GetPerson>(responseFromServer);
                if (results.ItemPath != null)
                {
                    response.Close();
                    return results.ItemPath;
                }
            }
            response.Close();
            return string.Empty;
        }

        private string Authenticate()
        {
            var authData = new Authentication()
            {
                Domain = Environment.GetEnvironmentVariable("Application_User_Domain"),
                Username = Environment.GetEnvironmentVariable("Application_User_Name"),
                Password = Environment.GetEnvironmentVariable("Application_User_Password"),
            };

            var sitecoreUri = Environment.GetEnvironmentVariable("Application_CMS_URL");

            var AuthUrl = $"{sitecoreUri}{SSCAPIs.AuthenticationApi}";

            var authRequest = (HttpWebRequest)WebRequest.Create(AuthUrl);
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";

            var requestAuthBody = JsonConvert.SerializeObject(authData);
            var authDatas = new UTF8Encoding().GetBytes(requestAuthBody);

            using (var dataStream = authRequest.GetRequestStream())
            {
                dataStream.Write(authDatas, 0, authDatas.Length);
            }

            CookieContainer cookies = new CookieContainer();
            authRequest.CookieContainer = cookies;

            using (var authResponse = authRequest.GetResponse())
            {
                _logger.LogDebug($"Login Status:\n\r{((HttpWebResponse)authResponse).StatusDescription}");
                return $".AspNet.Cookies={CookieValue(authResponse.Headers["Set-Cookie"], ".AspNet.Cookies")}";
            }
        }

        private string CookieValue(string header, string name)
        {
            Match M = Regex.Match(header, string.Format("{0}=(?<value>.*?);", name));
            return (M.ToString().Split('=')[1]);
        }
    }
}
