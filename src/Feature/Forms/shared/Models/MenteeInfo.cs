using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvp.Feature.Forms.Shared.Models
{
    public class MenteeInfo
    {
        
        public ApplicationStep ApplicationStep { get; set; }
        public Mentee Mentee { get; set; }
        public Person Person { get; set; }

        public MenteeStatus Status { get; set; }
    }

    public enum MenteeStatus
	{
        NotLoggedIn = -1,
        PersonItemNotFound = 0,
        ApplicationItemNotFound=1,
        ApplicationFound =2,
        ApplicationCompleted = 3
    }
    public class MenteeLists {
        public IEnumerable<Country> Country { get; set; }
        public IEnumerable<EmploymentStatus> EmploymentStatus { get; set; }
        public IEnumerable<MVPCategory> MVPCategory { get; set; }
    }
}