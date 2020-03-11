
using CnMedicineServer.Models;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace CnMedicineServer.Controllers
{
    /// <summary>
    /// 获取一些公共的配置信息的控制器。
    /// </summary>
    [RoutePrefix("api/Configure")]
    [EnableCors("*", "*", "*")/*crossDomain: true,*/]
    public class ConfigureController : OwApiControllerBase
    {
        public const string StoreServerUrl = "http://yun.pandaglb.com:10081";
        /// <summary>
        /// 获取上传文件等资源的地址。
        /// 集中管理文件资源的服务器应该要考虑存储的配置，而不能和普通Web服务器相同，故此需要考虑上传文件的服务器并非是Web服务器。
        /// 参数目前保留固定为"multipart/form-data",注意添加 enctype="multipart/form-data"
        /// </summary>
        /// <param name="contentType">当前版本保留为"multipart/form-data"</param>
        /// <returns>上传资源的地址。</returns>
        [Route("FileUploadPath")]
        [HttpGet]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetFileUploadPath(string contentType)
        {
            var ub = new UriBuilder();
            return Ok($"{StoreServerUrl}/api/Resource/UploadWithFormData");
        }
    }

    /// <summary>
    /// 管理文件等大资源的控制器。
    /// </summary>
    [RoutePrefix("api/Resource")]
    [EnableCors("*", "*", "*")/*crossDomain: true,*/]
    public class ResourceController : OwApiControllerBase
    {
        /// <summary>
        /// 存储资源文件的路径。
        /// </summary>
        public const string StorePath = "Content/UploadedFiles";

        /// <summary>
        /// 上传文件的操作。
        /// </summary>
        /// <returns></returns>
        [Route("UploadWithFormData")]
        [HttpPost]
        [ResponseType(typeof(List<ResourceStore>))]
        public async Task<HttpResponseMessage> PostUploadWithFormData()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string root = System.Web.HttpContext.Current.Server.MapPath($"~/{StorePath}");

            if (!Directory.Exists(root))    //若不存在目录
                Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);
                // This illustrates how to get the file names.
                //foreach (MultipartFileData file in provider.FileData)
                //{

                //    Trace.WriteLine(file.Headers.ContentDisposition.FileName);
                //    Trace.WriteLine("Server file path: " + file.LocalFileName);

                //}

                var result = new List<ResourceStore>();
                foreach (var item in provider.FileData)
                {
                    // provider.FileData[0].Headers.ContentDisposition.FileName;
                    var rs = new ResourceStore();
                    var extName = Path.GetExtension(item.Headers.ContentDisposition.FileName.Trim('\"'));   //取扩展名
                    var newFileName = $"{rs.Id.ToString("N")}{extName}";   //新文件名
                    rs.Path = $"{StorePath}/{newFileName}";
                    rs.Url = $"{ConfigureController.StoreServerUrl}/{rs.Path}";
                    File.Move(item.LocalFileName, Path.Combine(Path.GetDirectoryName(item.LocalFileName), newFileName));
                    result.Add(rs);
                }
                var db = DbContext;
                db.ResourceStories.AddRange(result);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }

}