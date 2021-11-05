using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCManukauTech.Models;

namespace MVCManukauTech.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private WorkDiaryEntities db = new WorkDiaryEntities();

        public ActionResult Index()
        {
            //20170317 JPC pass message from other pages
            if (TempData["Message"] != null)
            {
                ViewData["Message"] = TempData["Message"];
            }
                       
            //string sql = "SELECT Project.ProjectId AS ProjectId, [Name], Description, ExpectedHours, UserName "
            //    + "FROM ProjectMember INNER JOIN Project ON ProjectMember.ProjectId = Project.ProjectId ";

            //20170219 JPC added ProjectMemberId to this SELECT List because we need it to select the Diary List 
            //  for a logged-in student. This also means that we need to edit ViewModel "ProjectWithMember"
            //  and rename it to "ProjectWithMemberViewModel"
            string sql = "SELECT Project.ProjectId AS ProjectId, [Name], Description, ExpectedHours, UserName, ProjectMemberId "
                + "FROM Project LEFT OUTER JOIN ProjectMember ON Project.ProjectId = ProjectMember.ProjectId "
                + "ORDER BY ProjectId, UserName";

            //20170222 JPC for students add "WHERE UserName" clause and include NULL option for joining assignment
            //20170223 JPC this did not work as in UserName IS NULL only works for empty projects
            //sql += "WHERE (UserName = @p0 OR UserName IS NULL) ORDER BY [Name]";

            List<ProjectWithMemberViewModel> projectView 
                = db.Database.SqlQuery<ProjectWithMemberViewModel>(sql, User.Identity.Name).ToList();
            if (projectView.Count == 0 && User.IsInRole("Admin"))
            {
                ViewData["Message"] = "You have no Projects. You can use the link above to create them.";
            }
            else if (projectView.Count == 0)
            {
                ViewData["Message"] = "There are no projects available. Please check with your Lecturer or check back here later.";
                
            }

            if (!User.IsInRole("Admin"))
            {
                //Student List needs rewrite
                bool bFound = false; //looking for student is a member of a project

                //Scan the List and tag it
                int trackProjectId = projectView[0].ProjectId;
                for (int i = 0; i < projectView.Count; i++)
                {
                    if (projectView[i].UserName == User.Identity.Name)
                    {
                        bFound = true;
                    }
                    else
                    {
                        projectView[i].UserName = "NULL_DO_NOT_DISPLAY";
                    }

                    //Change of ProjectId
                    if(projectView[i].ProjectId != trackProjectId)
                    {
                        if(!bFound)
                        {
                            //20170322 JPC add condition for labelling statement
                            if (projectView[i - 1].UserName != User.Identity.Name)
                            {
                                projectView[i - 1].UserName = "TODO_JOIN_AS_MEMBER";
                            }     
                        }
                        //Reset bFound for new ProjectId found
                        bFound = false;
                        //20170322 JPC bug fix reset trackProjectId 
                        //now that we are tracking a different Project
                        trackProjectId = projectView[i].ProjectId;
                    }
                    if((i == projectView.Count - 1) && !bFound)
                    {
                        projectView[i].UserName = "TODO_JOIN_AS_MEMBER";
                    }
                }

            }
            return View(projectView);
        }

        //20170219 JPC Students choose which project they are doing "Join this Project"
        public ActionResult JoinThisProject(int id)
        {
            string sql = "INSERT INTO ProjectMember(ProjectId, UserName) VALUES(@p0, @p1)";
            int writeCheck = db.Database.ExecuteSqlCommand(sql, id, User.Identity.Name);
            if(writeCheck == 1)
            {
                //Success - one new row has been written into table ProjectMember
                //Display is red by default so a SUCCESS message needs a touch of green
                ViewData["Message"] = "<span style='color:darkgreen'>SUCCESS: your diary link is now available</span>";
            }
            else
            {
                ViewData["Message"] = "ERROR: project join did not happen - please contact your Lecturer or Admin.";
            }
            //Back to Index
            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
