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
    [Authorize(Roles ="Admin")]
    public class ProjectsController : Controller
    {
        private WorkDiaryEntities db = new WorkDiaryEntities();

        //GET: Projects
        public ActionResult Index()
        {
            string sql = "SELECT * FROM Project ORDER BY [Name]";
            List<Project> projects = db.Projects.SqlQuery(sql).ToList();
            return View(projects);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            string sql = "";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Now we have eliminated the null possibility, we need to "cast" id to a simple int projectId
            int projectId = (int)id;
            List<ProjectWithMemberViewModel> projectView = GetProjectMembers(projectId);
            if (projectView == null)
            {
                return HttpNotFound();
            }
            //A simple but less efficient way of getting an extra list to the view
            //when we have more than one list.
            ViewData["ProjectView"] = projectView;

            sql = "SELECT * FROM Category WHERE ProjectId = @p0";
            List<Category> category = db.Database.SqlQuery<Category>(sql, projectId).ToList();
            return View(category);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Name, string Description, float ExpectedHours)
        {
            Project project = new Models.Project();
            project.Name = Name;
            project.Description = Description;
            project.ExpectedHours = ExpectedHours;

            //20170212 JPC more versatile version of new record for a table
            // Avoids use of Model for versatility eg to make this work in cases like AJAX
            // However does make use of LINQ Add and SaveChanges as easier to code than SQL INSERT
            //20170516 JPC problems with LINQ change back to SQL!!
            //20170516 JPC no! it was need for RedirectToAction
            try
            {
                //string sql = "INSERT INTO Project([Name], Description, ExpectedHours) VALUES(@p0, @p1, @p2)";
                //int rowsChanged = db.Database.ExecuteSqlCommand(sql, Name, Description, ExpectedHours);
                db.Projects.Add(project);
                int rowsChanged = db.SaveChanges();
                if (rowsChanged != 1) throw new Exception("This project did not add as New");
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ViewData["Message"] = ex.Message;
                return View(project);
            }
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //DOES NOT WORK Project project = db.Projects.Find(id);
            //Back to SQL
            int projectId = (int)id;
            string sql = "SELECT * FROM Project WHERE ProjectId = @p0";
            //20170218 JPC NOTE we want only one SINGLE project rather than a list of them
            Project project = db.Projects.SqlQuery(sql, projectId).Single();
            if (project == null)
            {
                return HttpNotFound();
            }
            //Extra information - show who we have in this project
            //Editing on this same page is a challenge so will go for a separate page
            List<ProjectWithMemberViewModel> projectView = GetProjectMembers(projectId);
            if (projectView == null)
            {
                return HttpNotFound();
            }
            //A simple but less efficient way of getting an extra list to the view
            //when we have more than one list.
            ViewData["ProjectView"] = projectView;

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProjectId,Name,Description,ExpectedHours")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                //This ProjectsController does not have its own Index ("Home") page, 
                // - "Home" needs to be the master "Home" for all of this App
                return RedirectToAction("Index", "Home");
            }
            return View(project);
        }

        //---------------------------------------------------
        //Custom private functions - code reusable in more than one method above
        private List<ProjectWithMemberViewModel> GetProjectMembers(int projectId)
        {
            string sql = "SELECT Project.ProjectId AS ProjectId, [Name], Description, ExpectedHours, UserName "
                + "FROM ProjectMember INNER JOIN Project ON ProjectMember.ProjectId = Project.ProjectId "
                + "WHERE Project.ProjectId = @p0";
            List<ProjectWithMemberViewModel> projectView
                = db.Database.SqlQuery<ProjectWithMemberViewModel>(sql, projectId).ToList();
            return projectView;   
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
