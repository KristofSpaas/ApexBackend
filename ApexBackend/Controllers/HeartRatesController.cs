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
    public class HeartRatesController : ApiController
    {
        private ApplicationDbContext db;

        public HeartRatesController()
        {
            db = new ApplicationDbContext();
        }

        public HeartRatesController(ApplicationDbContext context)
        {
            db = context;
        }

    // GET: api/HeartRates/Patient/5
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/HeartRates/Patient/{patientId}")]
        public IHttpActionResult GetHeartRatesByPatientId(int patientId)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            List<HeartRate> heartRatesByPatientId = db.HeartRates.Where(r => r.PatientId == patientId).ToList();

            return Ok(heartRatesByPatientId);
        }

        // POST: api/HeartRates
        [Authorize(Roles = "Patient")]
        [Route("api/HeartRates")]
        [ResponseType(typeof (HeartRate))]
        public IHttpActionResult PostHeartRate(HeartRate heartRate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Patient patient = db.Patients.Find(heartRate.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + heartRate.PatientId + " does not exist.");
            }

            db.HeartRates.Add(heartRate);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new {controller = "heartrates", id = heartRate.HeartRateId}, heartRate);
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