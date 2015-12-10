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
    [Authorize(Roles = "Admin")]
    public class DoctorsController : ApiController
    {
        private ApplicationDbContext db;

        public DoctorsController()
        {
            db = new ApplicationDbContext();
        }

        public DoctorsController(ApplicationDbContext context)
        {
            db = context;
        }

        // GET: api/Doctors
        public List<Doctor> GetDoctors()
        {
            return db.Doctors.ToList();
        }

        // GET: api/Doctors/5
        [ResponseType(typeof (Doctor))]
        public IHttpActionResult GetDoctor(int id)
        {
            Doctor doctor = db.Doctors.Find(id);
            if (doctor == null)
            {
                return BadRequest("Doctor with id " + id + " does not exist.");
            }

            return Ok(doctor);
        }

        // PUT: api/Doctors/5
        [ResponseType(typeof (void))]
        public IHttpActionResult PutDoctor(int id, EditDoctorBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.DoctorId)
            {
                return BadRequest("The DoctorId in the URL and the DoctorId in the data do not match.");
            }

            var doctor = db.Doctors.Find(model.DoctorId);
            if (doctor == null)
            {
                return BadRequest("Doctor with id " + model.DoctorId + " does not exist.");
            }

            var user = db.Users.Find(doctor.UserId);
            if (user == null)
            {
                return BadRequest("User with id " + doctor.UserId + " does not exist.");
            }

            doctor.FirstName = model.FirstName;
            doctor.LastName = model.LastName;

            user.Email = model.Email;
            user.UserName = model.Email;

            db.Entry(doctor).State = EntityState.Modified;
            db.Entry(user).State = EntityState.Modified;

            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new {id = doctor.DoctorId},
                doctor);
        }

        // DELETE: api/Doctors/5
        [ResponseType(typeof (Doctor))]
        public IHttpActionResult DeleteDoctor(int id)
        {
            Doctor doctor = db.Doctors.Find(id);
            if (doctor == null)
            {
                return BadRequest("Doctor with id " + id + " does not exist.");
            }

            var user = db.Users.Find(doctor.UserId);

            if (user == null)
            {
                return BadRequest("User with id " + doctor.UserId + " does not exist.");
            }

            db.Doctors.Remove(doctor);
            db.Users.Remove(user);

            db.SaveChanges();

            return Ok(doctor);
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