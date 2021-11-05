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
    public class CategoriesController : Controller
    {
        private WorkDiaryEntities db = new WorkDiaryEntities();

        // GET: Categories
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int projectId = (int)id;
            ViewData["ProjectId"] = projectId;
            string sql = "SELECT * FROM Category WHERE ProjectId = @p0";
            List<Category> categories = db.Categories.SqlQuery(sql, projectId).ToList();

            //some helpful added info
            sql = "SELECT ([Name] + ' - ' + Description) AS ProjectAbout FROM Project WHERE ProjectId = @p0";
            string projectAbout = db.Database.SqlQuery<string>(sql, (int)id).Single();
            ViewData["ProjectAbout"] = projectAbout;

            return View(categories);
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create(int? id)
        {
            //20170218 JPC include ProjectId information
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewData["ProjectId"] = (int)id;
            //Lookup - NOTE - sending HTML code to the page requires method on the page Html.Raw()
            // to prevent removal for security reasons
            string sql = "SELECT ([Name] + '<br/>' + Description) AS ProjectAbout FROM Project WHERE ProjectId = @p0";
            string projectAbout = db.Database.SqlQuery<string>(sql, (int)id).Single();
            ViewData["ProjectAbout"] = projectAbout;
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,ProjectId,Name,Description,EstHours")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                //20170218 JPC HOWTO cope with the need to start again at index with a parameter
                //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
                return RedirectToAction("Index", new { id = category.ProjectId });
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,ProjectId,Name,Description,EstHours")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                //20170218 JPC HOWTO cope with the need to start again at index with a parameter
                //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
                return RedirectToAction("Index", new { id = category.ProjectId });
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            //20170218 JPC HOWTO cope with the need to start again at index with a parameter
            //  ref: http://stackoverflow.com/questions/1257482/redirecttoaction-with-parameter
            return RedirectToAction("Index", new { id = category.ProjectId });
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
