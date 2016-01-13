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

            foreach (HeartRate heartRate in heartRatesByPatientId)
            {
                heartRate.Patient = null;
            }

            return Ok(heartRatesByPatientId);
        }

        // GET: api/HeartRates/Patient/5/13334262668
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/HeartRates/Patient/{patientId}/{dateMillis}")]
        public IHttpActionResult GetHeartRatesOfDayByPatientId(int patientId, long dateMillis)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            var beginDate = GetStartOfDay(dateMillis);
            var endDate = GetEndOfDay(dateMillis);            

            List<HeartRate> heartRatesByPatientId = db.HeartRates.Where(
                r => r.PatientId == patientId && r.DateMillis > beginDate && r.DateMillis < endDate).ToList();

            foreach (HeartRate heartRate in heartRatesByPatientId)
            {
                heartRate.Patient = null;
            }

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

            var lastRecord = db.HeartRates
               .OrderByDescending(p => p.HeartRateId)
               .FirstOrDefault();

            if (lastRecord != null)
            {
                if (lastRecord.DateMillis == heartRate.DateMillis)
                {
                    return BadRequest("Record already exists");
                }
            }

            db.HeartRates.Add(heartRate);
            db.SaveChanges();

            heartRate.Patient = null;

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

        private long GetStartOfDay (long dateMillis)
        {
            var date = new DateTime((dateMillis * 10000) + 621355968000000000);
            var beginDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            var begin = beginDate.AddDays(1);

            var beginMillis = (long)(begin - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return beginMillis;
        }

        private long GetEndOfDay(long dateMillis)
        {
            var date = new DateTime((dateMillis * 10000) + 621355968000000000);
            var beginDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            var endDate = beginDate.AddDays(2);
        
            var endMillis = (long)(endDate - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return endMillis;
        }

    }
}