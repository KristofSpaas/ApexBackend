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
    public class MoodRatingsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/MoodRatings/Patient/5
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/MoodRatings/Patient/{patientId}")]
        public IHttpActionResult GetMoodRatingsByPatientId(int patientId)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            List<MoodRating> moodRatingsByPatientId = db.MoodRatings.Where(r => r.PatientId == patientId).ToList();

            foreach (MoodRating moodRating in moodRatingsByPatientId)
            {
                moodRating.Patient = null;
            }

            return Ok(moodRatingsByPatientId);
        }

        // GET: api/MoodRatings/Patient/5/13334262668
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/MoodRatings/Patient/{patientId}/{dateMillis}")]
        public IHttpActionResult GetMoodRatingsOfMonthByPatientId(int patientId, long dateMillis)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            var beginDate = GetStartOfMonth(dateMillis);
            var endDate = GetEndOfMonth(dateMillis);

            List<MoodRating> moodRatingsByPatientId = db.MoodRatings.Where(
                r => r.PatientId == patientId && r.DateMillis > beginDate && r.DateMillis < endDate).ToList();

            foreach (MoodRating moodRating in moodRatingsByPatientId)
            {
                moodRating.Patient = null;
            }

            return Ok(moodRatingsByPatientId);
        }

        // POST: api/MoodRatings
        [Authorize(Roles = "Patient")]
        [Route("api/MoodRatings")]
        [ResponseType(typeof(HeartRate))]
        public IHttpActionResult PostMoodRating(MoodRating moodRating)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Patient patient = db.Patients.Find(moodRating.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + moodRating.PatientId + " does not exist.");
            }

            var lastRecord = db.MoodRatings
               .OrderByDescending(p => p.MoodRatingId)
               .FirstOrDefault();

            if (lastRecord != null)
            {
                if (lastRecord.DateMillis == moodRating.DateMillis)
                {
                    return BadRequest("Record already exists");
                }
            }

            db.MoodRatings.Add(moodRating);
            db.SaveChanges();

            moodRating.Patient = null;

            return CreatedAtRoute("DefaultApi", new { controller = "moodratings", id = moodRating.MoodRatingId }, moodRating);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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