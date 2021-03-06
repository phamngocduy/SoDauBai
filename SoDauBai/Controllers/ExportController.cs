﻿using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Data.Entity;
using System.Reflection;

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

        public ActionResult Backup()
        {
            using (var db = new SoDauBaiEntities())
            {
                var database = new
                {
                    BanCanSu = db.BanCanSus.ToList(),
                    CauHinh = db.CauHinhs.ToList(),
                    GiangVien = db.GiangViens.ToList(),
                    GiaoVu = db.GiaoVus.ToList(),
                    GuiEmail = db.GuiEmails.ToList(),
                    LienHe = db.LienHes.ToList(),
                    NganhHoc = db.NganhHocs.ToList(),
                    NhanXet = db.NhanXets.ToList(),
                    PhongDayBu = db.PhongDayBus.ToList(),
                    PhuGiang = db.PhuGiangs.ToList(),
                    SoGhiBai = db.SoGhiBais.ToList(),
                    SoGhiChu = db.SoGhiChus.ToList(),
                    ThoiKhoaBieu = db.ThoiKhoaBieux.ToList(),
                    TroGiang = db.TroGiangs.ToList()
                };

                var path = Server.MapPath("~/App_Data");
                path = Path.Combine(path, "Backup" + DateTime.Today.ToString("yyMMdd"));
                using (var file = new StreamWriter(path, false))
                {
                    file.Write(JsonConvert.SerializeObject(database,
                        new JsonSerializerSettings { ContractResolver = new VirtualContractResolver() }));
                }
                return View();
            }
        }

        public ActionResult Index()
        {
            using (var db = new SoDauBaiEntities())
                return View(db.NganhHocs.ToList());
        }

        public ActionResult Details(int id)
        {
            using (var db = new SoDauBaiEntities())
            {
                var nganhHoc = db.NganhHocs.Find(id);
                ViewBag.GVs = db.GiangViens.ToList();
                var hocKy = Extension.GetHocKy(null, db);
                ViewBag.MaNganh = nganhHoc.MaNganh; ViewBag.HocKy = hocKy;
                return View(db.ThoiKhoaBieux.Include(tkb => tkb.SoGhiBais).Where(tkb => 
                    tkb.MaNganh == nganhHoc.MaNganh && tkb.HocKy == hocKy).ToList());
            }
        }
    }

    public class VirtualContractResolver : DefaultContractResolver
    {
        public static readonly VirtualContractResolver Instance = new VirtualContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance =>
            {
                return property.DeclaringType.GetProperty(property.PropertyName).GetGetMethod().IsVirtual == false;
            };
            return property;
        }
    }
}