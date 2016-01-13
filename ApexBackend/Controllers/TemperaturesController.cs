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
    public class TemperaturesController : ApiController
    {
        private ApplicationDbContext db;

        public TemperaturesController()
        {
            db = new ApplicationDbContext();
        }

        public TemperaturesController(ApplicationDbContext context)
        {
            db = context;
        }

        // GET: api/Temperatures/Patient/5
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/Temperatures/Patient/{patientId}")]
        public IHttpActionResult GetTemperaturesByPatientId(int patientId)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            List<Temperature> temperaturesByPatientId = db.Temperatures.Where(r => r.PatientId == patientId).ToList();

            foreach (Temperature temperature in temperaturesByPatientId)
            {
                temperature.Patient = null;
            }

            return Ok(temperaturesByPatientId);
        }

        // GET: api/Temperatures/Patient/5/13334262668
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/Temperatures/Patient/{patientId}/{dateMillis}")]
        public IHttpActionResult GetTemperaturesOfDayByPatientId(int patientId, long dateMillis)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            var beginDate = GetStartOfDay(dateMillis);
            var endDate = GetEndOfDay(dateMillis);

            List<Temperature> temperaturesByPatientId = db.Temperatures.Where(
                r => r.PatientId == patientId && r.DateMillis > beginDate && r.DateMillis < endDate).ToList();

            foreach (Temperature temperature in temperaturesByPatientId)
            {
                temperature.Patient = null;
            }

            return Ok(temperaturesByPatientId);
        }

        // POST: api/Temperatures
        [Authorize(Roles = "Patient")]
        [Route("api/Temperatures")]
        [ResponseType(typeof (Temperature))]
        public IHttpActionResult PostTemperature(Temperature temperature)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Patient patient = db.Patients.Find(temperature.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + temperature.PatientId + " does not exist.");
            }

            var lastRecord = db.Temperatures
               .OrderByDescending(p => p.TemperatureId)
               .FirstOrDefault();

            if (lastRecord != null)
            {
                if (lastRecord.DateMillis == temperature.DateMillis)
                {
                    return BadRequest("Record already exists");
                }
            }

            db.Temperatures.Add(temperature);
            db.SaveChanges();

            temperature.Patient = null;

            return CreatedAtRoute("DefaultApi", new {controller = "temperatures", id = temperature.TemperatureId},
                temperature);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private long GetStartOfDay(long dateMillis)
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