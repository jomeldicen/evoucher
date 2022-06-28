using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Helper
{
    public class ImageHelper
    {
        private string DefaultIcon = "";

        public ImageHelper()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.DefaultIcon = db.Settings.Where(x => x.vSettingID == "INV2X44R-4412-XR22-VXFG-BKLS19JOMEL").FirstOrDefault().vSettingOption; // Default Icon
            }
        }

        public string UploadImage(string path, string photo, string name)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath(string.Concat("~", path));
                string imageName = string.Concat(name, ".", photo.Split(';')[0].Split('/')[1]).ToLower();
                string imgPath = Path.Combine(sPath, imageName);

                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);

                if (photo.IndexOf(',') > -1)
                {
                    var imgStr = photo.Split(',')[1];
                    byte[] imageBytes = Convert.FromBase64String(imgStr);
                    File.WriteAllBytes(imgPath, imageBytes);
                }

                return string.Concat(path, imageName);
            }
            catch
            {
                return null;
            }
        }

        public string CopyImage(string source, string path, string name)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath(string.Concat("~", path));
                string destFile = Path.Combine(sPath, name);

                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);

                File.Copy(source, destFile, true);

                return string.Concat(path, name);
            }
            catch
            {
                return null;
            }
        }

        public string GenerateFile(string path, byte[] fileBytes, string name)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath(string.Concat("~", path));
                string filePath = Path.Combine(sPath, name);

                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);

                File.WriteAllBytes(filePath, fileBytes);

                return string.Concat(path, name);
            }
            catch
            {
                return null;
            }
        }

        // Convert Image to Base64 String
        public string GenerateBase64Str(string path)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath(string.Concat("~", path));

                byte[] imageArray = File.ReadAllBytes(sPath);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);

                return base64ImageRepresentation;
            }
            catch
            {
                return null;
            }
        }

        // QRCoder is a simple library, written in C#.NET, which enables you to create QR codes
        public string GenerateQRCode(string UrlLink)
        {
            string qrcodeBase64 = "";
            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UrlLink, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    qrcodeBase64 = Convert.ToBase64String(ms.ToArray());
                    bitMap.Dispose();
                }

                qrGenerator.Dispose();
                qrCodeData.Dispose();
                qrCode.Dispose();
                ms.Dispose();
            }

            return qrcodeBase64;
        }
    }
}