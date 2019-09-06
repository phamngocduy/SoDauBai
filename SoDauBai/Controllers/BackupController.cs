using Quartz;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Mvc;
using SoDauBai.Models;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace SoDauBai.Controllers
{
    public class BackupController : Controller, IJob
    {
        string JSON = null;

        public async Task<ContentResult> Execute()
        {
            await Execute(null);
            return Content(JSON, "application/json");
        }

        [HttpPost]
        public async Task Execute(IJobExecutionContext context)
        {
            using (var db = new SoDauBaiEntities())
            {
                var database = new
                {
                    GiangVien = db.GiangViens.ToList(),
                    GiaoVu = db.GiaoVus.ToList(),
                    LienHe = db.LienHes.ToList(),
                    NganhHoc = db.NganhHocs.ToList(),
                    NhanXet = db.NhanXets.ToList(),
                    PhongDayBu = db.PhongDayBus.ToList(),
                    SoGhiBai = db.SoGhiBais.ToList(),
                    ThoiKhoaBieu = db.ThoiKhoaBieux.ToList()
                };

                var path = Server.MapPath("~/App_Data");
                path = Path.Combine(path, "Backup" + DateTime.Today.ToString("yyMMdd"));
                using (var file = new StreamWriter(path, false))
                {
                    await file.WriteAsync(JSON = JsonConvert.SerializeObject(database,
                        new JsonSerializerSettings { ContractResolver = new VirtualContractResolver() }));
                }
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