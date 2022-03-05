using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvp.Feature.Forms.Constants
{
    public struct Person
    {
        public struct Folder
        {
            public static ID FOLDER_ID = ID.Parse("{64F31E3A-2040-4E69-B9A7-6830CBE669D2}");//this is the content folder?
        }
        public struct FolderParentMvp
        {
            public static ID FOLDER_ID = ID.Parse("{6AD2AA25-E648-4D12-BB4F-4DFEF7B27046}");//this is the content folder?
        }
        public struct Template
        {
            public static ID TEMPLATE_ID = ID.Parse("{AD9C7837-8660-4360-BA2B-7ADDF4163685}");
        
            public struct Fields
            {
                public static ID OKTA_ID = ID.Parse("{9D7006BB-7BD1-4D17-BF85-C4075918659F}");
                public static ID PEOPLE_FIRST_NAME = ID.Parse("{022DE4C6-82CF-437C-9424-2FDDF1A94F68}");
                public static ID PEOPLE_LAST_NAME = ID.Parse("{C0B5D514-19C3-430E-9A5A-42763211573D}");
                public static ID PEOPLE_EMAIL = ID.Parse("{8561A65D-741E-4EAD-985E-EA1880E196A9}");
                public static ID PEOPLE_APPLICATION = ID.Parse("{55701D4E-E6FD-4340-83AC-7DC3265DA612}");
                public static ID PEOPLE_APPLICATION_STEP = ID.Parse("{512180D8-71CF-4192-A692-36211100557E}");
                public static ID PEOPLE_COUNTRY = ID.Parse("{046FB914-F54F-4E3A-85E8-8EA5E3B7DB7F}");
                public static ID PEOPLE_CATEGORY = ID.Parse("{665EF327-D5E3-4CC0-B5CA-21102FCCC7D2}");               

            }
        }
    }
}