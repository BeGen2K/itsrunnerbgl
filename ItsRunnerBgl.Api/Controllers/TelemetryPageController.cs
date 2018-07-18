/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItsRunnerBgl.Api.Controllers
{
    public class TelemetryPageController : Controller
    {
        // GET: TelemetryPage
        public ActionResult Index()
        {
            return View();
        }

        // GET: TelemetryPage/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TelemetryPage/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TelemetryPage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TelemetryPage/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TelemetryPage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TelemetryPage/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TelemetryPage/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
*/