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
    public class AdvicesController : ApiController
    {
        private ApplicationDbContext db;

        public AdvicesController()
        {
            db = new ApplicationDbContext();
        }

        public AdvicesController(ApplicationDbContext context)
        {
            db = context;
        }

        // GET: api/Advices/5
        [ResponseType(typeof (Advice))]
        [Authorize(Roles = "Doctor")]
        [Route("api/Advices/{id}")]
        public IHttpActionResult GetAdvice(int id)
        {
            Advice advice = db.Advices.Find(id);
            if (advice == null)
            {
                return BadRequest("Advice with id " + id + " does not exist.");
            }

            return Ok(advice);
        }

        // GET: api/Advices/Patient/5
        [Authorize(Roles = "Doctor, Patient")]
        [Route("api/Advices/Patient/{patientId}")]
        public IHttpActionResult GetAdvicesByPatientId(int patientId)
        {
            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + patientId + " does not exist.");
            }

            List<Advice> advicesByPatientId = db.Advices.Where(r => r.PatientId == patientId).ToList();

            foreach (Advice advice in advicesByPatientId)
            {
                advice.Patient = null;
            }

            return Ok(advicesByPatientId);
        }

        // GET: api/Advices/Doctor/5
        [Authorize(Roles = "Doctor")]
        [Route("api/Advices/Doctor/{doctorId}")]
        public IHttpActionResult GetAdvicesByDoctorId(int doctorId)
        {
            Doctor doctor = db.Doctors.Find(doctorId);
            if (doctor == null)
            {
                return BadRequest("Doctor with id " + doctorId + " does not exist.");
            }

            List<Patient> patientsByDoctorId = db.Patients.Where(r => r.DoctorId == doctorId).ToList();

            List<Advice> advicesByDoctorId = new List<Advice>();

            foreach (Patient patient in patientsByDoctorId)
            {
                List<Advice> advicesByPatientId = db.Advices.Where(r => r.PatientId == patient.PatientId).ToList();

                foreach (Advice advice in advicesByPatientId)
                {
                    advicesByDoctorId.Add(advice);
                }
            }

            foreach (Advice advice in advicesByDoctorId)
            {
                advice.Patient = null;
            }

            return Ok(advicesByDoctorId);
        }

        // PUT: api/Advices/5
        [Authorize(Roles = "Doctor")]
        [Route("api/Advices/{id}")]
        [ResponseType(typeof (void))]
        public IHttpActionResult PutAdvice(int id, Advice advice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != advice.AdviceId)
            {
                return BadRequest("The AdviceId in the URL and the AdviceId in the data do not match.");
            }

            Patient patient = db.Patients.Find(advice.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + advice.PatientId + " does not exist.");
            }

            db.Entry(advice).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdviceExists(id))
                {
                    return BadRequest("Advice with id " + id + " does not exist.");
                }
                else
                {
                    throw;
                }
            }

            advice.Patient = null;

            return CreatedAtRoute("DefaultApi", new {controller = "advices", id = advice.AdviceId}, advice);
        }

        // POST: api/Advices
        [Authorize(Roles = "Doctor")]
        [Route("api/Advices")]
        [ResponseType(typeof (Advice))]
        public IHttpActionResult PostAdvice(Advice advice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Patient patient = db.Patients.Find(advice.PatientId);
            if (patient == null)
            {
                return BadRequest("Patient with id " + advice.PatientId + " does not exist.");
            }

            db.Advices.Add(advice);
            db.SaveChanges();

            advice.Patient = null;

            return CreatedAtRoute("DefaultApi", new {controller = "advices", id = advice.AdviceId}, advice);
        }

        // DELETE: api/Advices/5
        [Authorize(Roles = "Doctor")]
        [Route("api/Advices/{id}")]
        [ResponseType(typeof (Advice))]
        public IHttpActionResult DeleteAdvice(int id)
        {
            Advice advice = db.Advices.Find(id);
            if (advice == null)
            {
                return BadRequest("Advice with id " + id + " does not exist.");
            }

            db.Advices.Remove(advice);
            db.SaveChanges();

            return Ok(advice);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AdviceExists(int id)
        {
            return db.Advices.Count(e => e.AdviceId == id) > 0;
        }
    }
}