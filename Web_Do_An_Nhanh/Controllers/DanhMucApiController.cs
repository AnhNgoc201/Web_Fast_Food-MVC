using System.Linq;
using System.Web.Http;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class DanhMucApiController : ApiController
    {
        JOLLIBEEEntities db = new JOLLIBEEEntities();

        public DanhMucApiController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET: api/DanhMucApi
        // GET: api/DanhMucApi/1
        [HttpGet]
        public IHttpActionResult Get(int id = 0)
        {
            if (id == 0)
            {
                return Ok(db.DanhMucs.ToList());
            }

            var dm = db.DanhMucs.Find(id);
            if (dm == null)
                return NotFound();

            return Ok(dm);
        }

        // POST: api/DanhMucApi
        [HttpPost]
        public IHttpActionResult Post(DanhMuc dm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.DanhMucs.Add(dm);
            db.SaveChanges();

            return Ok(dm);
        }

        // PUT: api/DanhMucApi/1
        [HttpPut]
        public IHttpActionResult Put(int id, DanhMuc dm)
        {
            var d = db.DanhMucs.Find(id);
            if (d == null)
                return NotFound();

            d.TenDM = dm.TenDM;
            d.MoTa = dm.MoTa;

            db.SaveChanges();
            return Ok(d);
        }

        // DELETE: api/DanhMucApi/1
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var d = db.DanhMucs.Find(id);
            if (d == null)
                return NotFound();

            db.DanhMucs.Remove(d);
            db.SaveChanges();

            return Ok("Xóa danh mục thành công");
        }
        
    }
}
