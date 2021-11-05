using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//ViewModel to support list of projects including UserName

namespace MVCManukauTech.Models
{
    public class ProjectWithMemberViewModel
    {
 
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double ExpectedHours { get; set; }
        public string UserName { get; set; }

        //20170219 JPC add ProjectMemberId as int? to cover the case of LEFT JOIN resulting in NULL
        public int? ProjectMemberId { get; set; }
    }

}