using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    public class ExportController : Controller
    {
        public JsonResult getMajors()
        {
            using (var db = new SoDauBaiEntities())
                return Json(db.NganhHocs.OrderBy(n => n.TenNganh).Select(n => new
                {
                    Code = n.MaNganh.Trim(), Name = n.TenNganh.Trim()
                }).ToList(), JsonRequestBehavior.AllowGet);
        }

        private DayOfWeek getDayOfWeek(int thuKieuSo)
        {
            try
            {
                return (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), CONST.THU[thuKieuSo]);
            }
            catch (Exception)
            {
                return DayOfWeek.Sunday;
            }
        }

        private TimeSpan getTimeSpan(int tietBatDau)
        {
            try
            {
                return CONST.TIET[tietBatDau];
            }
            catch (Exception)
            {
                return CONST.TIET[0];
            }
        }

        public JsonResult getCourses()
        {
            using (var db = new SoDauBaiEntities())
            {
                var hk = db.ThoiKhoaBieux.MaxOrDefault(c => c.HocKy);
                var gv = db.GiangViens.ToList();
                return Json(new
                {
                    Semester = hk,
                    Courses = db.ThoiKhoaBieux.Where(c => c.HocKy == hk)
                    .OrderBy(c => c.TenMH).ToList().Select(c => new
                    {
                        Code = c.MaMH,
                        Name = c.TenMH,
                        Type1 = c.NhomTo,
                        Type2 = String.Format("{0}_{1}", c.ToTH, c.TenToHop),

                        Major = c.MaNganh,
                        Lecturer = (gv.SingleOrDefault(l => l.MaGV == c.MaGV)
                                    ?? new GiangVien()).Email,

                        Credit = c.SoTinChi,
                        Students = c.TongSoSV,

                        DayOfWeek = getDayOfWeek(c.ThuKieuSo).ToString(),
                        TimeSpan = getTimeSpan(c.TietBD).ToString(@"hh\:mm\:ss"),
                        Periods = c.SoTiet,

                        Room = c.MaPH,
                        Note = new
                        {
                            MaLop = c.MaLop,
                            TenLop = c.TenLop
                        }
                    }).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}