using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ApexBackend.Models;

namespace ApexBackend.Controllers
{
    [Authorize(Roles = "Doctor, Patient")]
    public class StepsPerDaysController : ApiController
    {
        private ApplicationDbContext db;

        public StepsPerDaysController()
        {
            db = new ApplicationDbContext();
        }

        public StepsPerDaysController(ApplicationDbContext context)
        {
            db = context;
        }

        // GET: api/StepsPerDays/Patient/5
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/StepsPerDays/Patient/{patientId}")]
        public IHttpActionResult GetStepsPerDaysByPatientId(int patientId)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            List<StepsPerDay> stepsPerDaysByPatientId = db.StepsPerDays.Where(r => r.PatientId == patientId).ToList();

            foreach (StepsPerDay steps in stepsPerDaysByPatientId)
            {
                steps.Patient = null;
            }

            return Ok(stepsPerDaysByPatientId);
        }

        // GET: api/StepsPerDays/Patient/5/13334262668
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/StepsPerDays/Patient/{patientId}/{dateMillis}")]
        public IHttpActionResult GetStepsPerDaysOfMonthByPatientId(int patientId, long dateMillis)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            var beginDate = GetStartOfMonth(dateMillis);
            var endDate = GetEndOfMonth(dateMillis);

            List<StepsPerDay> stepsPerDaysByPatientId = db.StepsPerDays.Where(
                r => r.PatientId == patientId && r.DateMillis > beginDate && r.DateMillis < endDate).ToList();

            foreach (StepsPerDay stepsPerDay in stepsPerDaysByPatientId)
            {
                stepsPerDay.Patient = null;
            }

            return Ok(stepsPerDaysByPatientId);
        }

        // PUT: api/StepsPerDays/5
        [Authorize(Roles = "Patient")]
        [Route("api/StepsPerDays/{id}")]
        [ResponseType(typeof (void))]
        public IHttpActionResult PutStepsPerDay(int id, StepsPerDay stepsPerDays)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stepsPerDays.StepsPerDayId)
            {
                return BadRequest("The StepsPerDayId in the URL and the StepsPerDayId in the data do not match.");
            }

            Patient patient = db.Patients.Find(stepsPerDays.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + stepsPerDays.PatientId + " does not exist.");
            }

            db.Entry(stepsPerDays).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StepsPerDaysExists(id))
                {
                    return BadRequest("StepsPerDay with id " + id + " does not exist.");
                }
                else
                {
                    throw;
                }
            }

            stepsPerDays.Patient = null;

            return CreatedAtRoute("DefaultApi", new {controller = "stepsperdays", id = stepsPerDays.StepsPerDayId},
                stepsPerDays);
        }

        // POST: api/StepsPerDays
        [Authorize(Roles = "Patient")]
        [Route("api/StepsPerDays")]
        [ResponseType(typeof (StepsPerDay))]
        public IHttpActionResult PostStepsPerDay(StepsPerDay stepsPerDay)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Patient patient = db.Patients.Find(stepsPerDay.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + stepsPerDay.PatientId + " does not exist.");
            }

            db.StepsPerDays.Add(stepsPerDay);
            db.SaveChanges();

            stepsPerDay.Patient = null;

            return CreatedAtRoute("DefaultApi", new {controller = "stepsperdays", id = stepsPerDay.StepsPerDayId},
                stepsPerDay);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StepsPerDaysExists(int id)
        {
            return db.StepsPerDays.Count(e => e.StepsPerDayId == id) > 0;
        }

        private long GetStartOfMonth(long dateMillis)
        {
            var date = new DateTime((dateMillis * 10000) + 621355968000000000);
            var beginDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            var begin = beginDate.AddDays(1);

            var beginMillis = (long)(begin - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return beginMillis;
        }

        private long GetEndOfMonth(long dateMillis)
        {
            var date = new DateTime((dateMillis * 10000) + 621355968000000000);
            var beginDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            var endDate = beginDate.AddMonths(1);
            endDate = endDate.AddDays(1);

            var endMillis = (long)(endDate - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return endMillis;
        }
    }
}