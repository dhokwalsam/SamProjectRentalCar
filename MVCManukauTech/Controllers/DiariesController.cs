using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCManukauTech.Models;
using System.Text.RegularExpressions;
using MVCManukauTech.ViewModels;

namespace MVCManukauTech.Controllers
{
    [Authorize]
    public class DiariesController : Controller
    {
        private WorkDiaryEntities db = new WorkDiaryEntities();

        // GET: Diaries
        public ActionResult Index()
        {
            //20170317 JPC if starting with a work diary, there will be a querystring id
            if(Request.QueryString["p"] == null && Session["ProjectMemberId"] == null)
            {
                TempData["Message"] = "ERROR: You need to select a valid diary link.";
                return RedirectToAction("Index", "Home");
            }
            if (Request.QueryString["p"] != null)
            {
                if(User.IsInRole("Admin"))
                {
                    //Admin engage directly with ProjectMemberId
                    Session["ProjectMemberId"] = Request.QueryString["p"];
                    //Lookup username
                    string sql = "SELECT Username FROM ProjectMember WHERE ProjectMemberId = @p0";
                    string projectMemberName = db.Database.SqlQuery<string>(sql, Request.QueryString["p"]).Single();
                    Session["ProjectMemberName"] = projectMemberName;
                }
                else
                {
                    //Possibility of an ordinary user seeing another user's work by changing the ProjectMemberId
                    //Therefore user selects ProjectId and we derive ProjectMemberId here.
                    //Starting work on work diary with ProjectId - we need to know ProjectMemberId
                    string projectId = Request.QueryString["p"];
                    string sql = "SELECT ProjectMemberId FROM ProjectMember WHERE ProjectId = @p0 AND Username = @p1";
                    try
                    {
                        int pid = db.Database.SqlQuery<int>(sql, projectId, User.Identity.Name).Single();
                        Session["ProjectMemberId"] = pid;
                        Session["ProjectMemberName"] = User.Identity.Name;
                    }
                    catch(Exception ex)
                    {
                        TempData["Message"] = "ERROR: You need to select a valid diary link.";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            if(TempData["Message"] != null)
            {
                ViewData["Message"] = TempData["Message"];
            }

            int id = Convert.ToInt32(Session["ProjectMemberId"]);
            //20170219 JPC note use of LINQ here where the generated code is modified by 
            // including the LINQ version of WHERE and ORDER BY ... DESC
            var diaries = db.Diaries.Include(d => d.Category).Include(d => d.ProjectMember).Where(d => d.ProjectMemberId == id).OrderByDescending(d => d.Time1).ToList();
            //20170301 JAK SON Take tags out of brief summary text
            //foreach (Diary item in diaries)
            //{
            //    if (item.Notes != null)
            //    {
            //        item.Notes = Regex.Replace(item.Notes, @"<[^>]+>|&nbsp;", "").Trim();
            //    }
            //}
            
            return View(diaries);

            //20170219 JPC what does the above LINQ look like in SQL?
            //  SELECT Diary.* FROM 
            //  (Diary INNER JOIN Category ON Diary.CategoryId = Category.CategoryId)
            //    INNER JOIN ProjectMember ON Diary.ProjectMemberId = ProjectMember.ProjectMemberId
            //  WHERE Diary.ProjectMemberId = @p0 ORDER BY Diary.Time1 DESC
        }


        // GET: Diaries/Create
        //20170219 JPC Adding use of ProjectMemberId. Use Categories as example to follow
        //   where the Foreign Key effect has already been worked out
     
        public ActionResult Create()
        {
            //20170316 JPC PSD change from querystring to Session for ProjectMemberId
            int id = Convert.ToInt32(Session["ProjectMemberId"]);
            //We need to persist ProjectMemberId in the form as a hidden field so it gets submitted
            //along with user input. Use ViewData as the value carrier to get it there.

            //20170226 JPC experiment, get specific and explicit about working with the model
            //by creating a new empty model and passing that to the page
            Diary diary = new Diary();
            //default date
            diary.Date = DateTime.Today;
            diary.Time1 = DateTime.Today;
            diary.Time2 = DateTime.Today;
            diary.ProjectMemberId = id;
            diary.Charge = "N";
            diary.XDateTime = DateTime.Now;
            diary.Status = "0";
            //20170316 JPC -- replace with Session -- ViewData["ProjectMemberId"] = id;

            //We need to know the ProjectId so that we have only the Categories for this Project
            string sql = "SELECT ProjectId FROM ProjectMember WHERE ProjectMemberId = @p0";
            int projectId = db.Database.SqlQuery<int>(sql, id).Single();
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.ProjectId == projectId), "CategoryId", "Name");
            return View(diary);
        }


        // POST: Diaries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DiaryId,Date,CategoryId,ProjectMemberId,Time1,Time2,Notes,Charge,XDateTime,Status")] Diary diary)
        {
            //20170303 JPC un-escape the hidden HTML
            string s = diary.Notes.Replace("&xlt;", "<");
            diary.Notes = s.Replace("&xgt;", ">");

            if (ModelState.IsValid)
            {
                db.Diaries.Add(diary);
                db.SaveChanges();
                //20170219 JPC HOWTO cope with the need to start again at index with a parameter
                //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
                //20170316 JPC remove id, Session takes over
                return RedirectToAction("Index");
            }
      
            //We need to know the ProjectId so that we have only the Categories for this Project
            string sql = "SELECT ProjectId FROM ProjectMember WHERE ProjectMemberId = @p0";
            //20170307 JPC add parameter for selected item diary.CategoryId
            int projectId = db.Database.SqlQuery<int>(sql, diary.ProjectMemberId).Single();
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.ProjectId == projectId), "CategoryId", "Name", diary.CategoryId);
            return View(diary);
        }

   
        // GET: Diaries/Edit/5
       
        public ActionResult Edit(int id)
        {
            Diary diary = db.Diaries.Find(id);
            //20170317 JPC security issue. What if user edits the URL to change the id?
            //Do a check to find out if this id belongs to this user
            if (diary == null || (diary.ProjectMemberId != Convert.ToInt32(Session["ProjectMemberId"])))
            {
                //User is attempting to edit a diary note that is not valid or not theirs
                //Could be inadvertent error, hacking or bug - in any case this needs to stop
                TempData["Message"] = "ERROR: Invalid selection. Please use the diary links below for navigation.";
                return RedirectToAction("Index");
            }

            //We need to know the ProjectId so that we have only the Categories for this Project
            string sql = "SELECT ProjectId FROM ProjectMember WHERE ProjectMemberId = @p0";
            int projectId = db.Database.SqlQuery<int>(sql, diary.ProjectMemberId).Single();
            //20170307 JPC add parameter for selected item diary.CategoryId
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.ProjectId == projectId), "CategoryId", "Name", diary.CategoryId);
            return View(diary);
        }

        // POST: Diaries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DiaryId,Date,CategoryId,ProjectMemberId,Time1,Time2,Notes,Charge,XDateTime,Status")] Diary diary)
        {
            //20170303 JPC un-escape the hidden HTML
            string s = diary.Notes.Replace("&xlt;", "<");
            diary.Notes = s.Replace("&xgt;", ">");

            if (ModelState.IsValid)
            {
                db.Entry(diary).State = EntityState.Modified;
                db.SaveChanges();
                //20170219 JPC HOWTO cope with the need to start again at index with a parameter
                //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
                return RedirectToAction("Index");
            }

            //We need to know the ProjectId so that we have only the Categories for this Project
            string sql = "SELECT ProjectId FROM ProjectMember WHERE ProjectMemberId = @p0";
            int projectId = db.Database.SqlQuery<int>(sql, diary.ProjectMemberId).Single();
            //20170307 JPC add parameter for selected item diary.CategoryId
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.ProjectId == projectId), "CategoryId", "Name", diary.CategoryId);
            return View();
        }

        // GET: Diaries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Diary diary = db.Diaries.Find(id);
            if (diary == null)
            {
                return HttpNotFound();
            }
            return View(diary);
        }

        // POST: Diaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Diary diary = db.Diaries.Find(id);
            db.Diaries.Remove(diary);
            db.SaveChanges();
            //20170219 JPC HOWTO cope with the need to start again at index with a parameter
            //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
            return RedirectToAction("Index");
        }

        //20170307 JPC Example of using a ViewModel to display a report
        public ActionResult MetricsReport()
        {
            //20170322 JPC display category subtotals rounded off to the nearest quarter hour
            string sql = @"SELECT Category.Name
            , CONVERT(FLOAT, (ROUND(Sum(DateDiff(minute, Time1, Time2))/15, 0))/4.0) AS HoursWorked, EstHours
            FROM Diary RIGHT JOIN Category ON Diary.CategoryId = Category.CategoryId
            WHERE Diary.ProjectMemberId = @p0 
            GROUP BY Diary.CategoryId, Category.Name, Category.EstHours";
            int pmId = Convert.ToInt32(Session["ProjectMemberId"]);
            List<MetricsReportViewModel> report 
                = db.Database.SqlQuery<MetricsReportViewModel>(sql, pmId).ToList();

            //20170322 JPC calculate TotalHoursWorked
            double totalHoursWorked = 0;
            foreach(MetricsReportViewModel item in report)
            {
                totalHoursWorked += item.HoursWorked;
            }
            ViewData["TotalHoursWorked"] = totalHoursWorked;
            return View(report);
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
