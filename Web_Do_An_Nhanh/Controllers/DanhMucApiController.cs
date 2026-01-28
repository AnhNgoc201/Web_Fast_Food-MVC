using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    [RoutePrefix("api/DanhMuc")]
    public class DanhMucApiController : ApiController
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        // =========================================
        // GET ALL: /api/DanhMuc
        // =========================================
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var list = db.DanhMucs
                .Select(dm => new {
                    dm.MaDM,
                    dm.TenDM,
                    dm.MoTa
                })
                .ToList();

            return Ok(list);
        }


        // =========================================
        // GET BY ID: /api/DanhMuc/{id}
        // =========================================
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var dm = db.DanhMucs
                .Where(x => x.MaDM == id)
                .Select(x => new {
                    x.MaDM,
                    x.TenDM,
                    x.MoTa
                })
                .FirstOrDefault();

            if (dm == null)
                return NotFound();

            return Ok(dm);
        }


        // =========================================
        // CREATE: POST /api/DanhMuc
        // =========================================
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(DanhMuc model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.DanhMucs.Add(model);
            db.SaveChanges();

            return Ok(new
            {
                message = "Thêm danh mục thành công!",
                data = model
            });
        }

        // =========================================
        // UPDATE: PUT /api/DanhMuc/{id}
        // =========================================
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, DanhMuc model)
        {
            var dm = db.DanhMucs.Find(id);
            if (dm == null)
                return NotFound();

            dm.TenDM = model.TenDM;
            dm.MoTa = model.MoTa;

            db.SaveChanges();

            return Ok(new
            {
                message = "Cập nhật danh mục thành công!",
                data = dm
            });
        }

        // =========================================
        // DELETE: DELETE /api/DanhMuc/{id}
        // =========================================
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var dm = db.DanhMucs.Find(id);
            if (dm == null)
                return NotFound();

            db.DanhMucs.Remove(dm);
            db.SaveChanges();

            return Ok(new
            {
                message = "Xóa danh mục thành công!"
            });
        }
    }
}

