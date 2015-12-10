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
            return db.Temperatures.Count(e => e.TemperatureId == id) > 0;
        }
    }
}