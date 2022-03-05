using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Mvp.Feature.Forms.Search;
using Mvp.Feature.Forms.Shared.Models;
using Sitecore.Data.Fields;

namespace Mvp.Feature.Forms.Controllers
{
    public class MenteeController : Controller
    {
        FormsService _service;
        public MenteeController()
        {
            _service = new FormsService();
        }

        [HttpPost]
        public JsonResult GetMenteeInfo(string identifier, string email)
        {

            var menteeInfoModel = new MenteeInfo();
            if (string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(email))
            {
                menteeInfoModel = new MenteeInfo
                {
                    Status = MenteeStatus.PersonItemNotFound
                };

                return Json(menteeInfoModel, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(identifier))
            {

                var personItem = _service.SearchPeopleByOktaId(identifier);

                //fallback to email verification assuming the persons okta id was updated, can be removed later
                if (personItem == null)
                {
                    personItem = _service.SearchPeopleByEmail(email);
                }

                if (personItem != null)
                {
                    Person personO = new Person();
                    personO.FirstName = personItem.Fields[Constants.Person.Template.Fields.PEOPLE_FIRST_NAME].Value;
                    personO.LastName = personItem.Fields[Constants.Person.Template.Fields.PEOPLE_LAST_NAME].Value;
                    personO.OktaId = personItem.Fields[Constants.Person.Template.Fields.OKTA_ID].Value;
                    personO.Email = personItem.Fields[Constants.Person.Template.Fields.PEOPLE_EMAIL].Value;
                    personO.ItemPath = personItem.Paths.FullPath;
                    personO.ItemId = personItem.ID.ToString();


                    var applicationItemId = personItem.Fields[Constants.Person.Template.Fields.PEOPLE_APPLICATION]?.Value;
                    var applicationModel = _service.GetMenteeModel(applicationItemId);



                    if (applicationModel != null)
                    {
                        if (applicationModel.Completed)
                        {
                            menteeInfoModel = new MenteeInfo
                            {
                                Status = MenteeStatus.ApplicationCompleted
                            };
                        }
                        else
                        {
                            var applicationStepId = applicationModel.Step;
                            ApplicationStep applicationStep = _service.GetApplicationStepModel(applicationStepId);

                            menteeInfoModel = new MenteeInfo
                            {
                                Mentee = applicationModel,
                                ApplicationStep = applicationStep,
                                Person = personO,
                                Status = MenteeStatus.ApplicationFound
                            };
                        }
                    }
                    else
                    {
                        menteeInfoModel = new MenteeInfo
                        {
                            Person = personO,
                            Status = MenteeStatus.ApplicationItemNotFound
                        };
                    }
                }
                else
                {
                    menteeInfoModel = new MenteeInfo
                    {
                        Status = MenteeStatus.PersonItemNotFound
                    };
                }
            }
            else
            {
                menteeInfoModel = new MenteeInfo
                {
                    Status = MenteeStatus.NotLoggedIn
                };
            }
            return Json(menteeInfoModel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetMentorLists()
        {
            {
                var mvps= _service.GetMvpItems();
                IList<Person> mvpsListResult= new List<Person>();
                foreach(var mvp in mvps)
                {
                    mvpsListResult.Add(new Person { FirstName = mvp[Constants.Person.Template.Fields.PEOPLE_FIRST_NAME], 
                        LastName = mvp[Constants.Person.Template.Fields.PEOPLE_LAST_NAME],
                        Email = mvp[Constants.Person.Template.Fields.PEOPLE_EMAIL],
                        CountryId = mvp[Constants.Person.Template.Fields.PEOPLE_COUNTRY], 
                        CategoryId = mvp[Constants.Person.Template.Fields.PEOPLE_CATEGORY] });
                }

                return Json(mvpsListResult, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetMenteeLists()
        {
            {
                var applicationListsModel = new ApplicationLists
                {
                    Country = _service.GetCountries(),
                    EmploymentStatus = _service.GetEmploymentStatus(),
                    MVPCategory = _service.GetMVPCategories(),
                };

                return Json(applicationListsModel, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult SubmitForm(string category, string countryBirth, string countryResidence, string techSkill, string firstName, string lastName)
        {
            _service.SaveMentee(category, countryBirth, countryResidence, techSkill, firstName, lastName);
            return Json("OK", JsonRequestBehavior.AllowGet);

        }
    }
}
 