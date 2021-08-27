using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Transfer;
using Newtonsoft.Json.Linq;

namespace EvernoteToNotion.Service
{
    class COSManager
    {
        private static CosXml _cosXml;
        private static string _bucket = "";
        private static string _appid = "";//设置腾讯云账户的账户标识 APPID
        private static string _region = ""; //设置一个默认的存储桶地域
        private static string _secretId = ""; //"云 API 密钥 SecretId";
        private static string _secretKey = ""; //"云 API 密钥 SecretKey";

        static COSManager()
        {

            if (!File.Exists("CosConfig.json"))
            {
                throw new Exception("CosConfig.json not found");
            }

            JObject configJson = JObject.Parse(File.ReadAllText("CosConfig.json"));

            _bucket = configJson["bucket"].ToString();
            _appid = configJson["appid"].ToString();//设置腾讯云账户的账户标识 APPID
            _region = configJson["region"].ToString(); //设置一个默认的存储桶地域
            _secretId = configJson["secretId"].ToString(); //"云 API 密钥 SecretId";
            _secretKey = configJson["secretKey"].ToString(); //"云 API 密钥 SecretKey";

            if (string.IsNullOrWhiteSpace(_bucket) ||
                string.IsNullOrWhiteSpace(_appid) ||
                string.IsNullOrWhiteSpace(_region) ||
                string.IsNullOrWhiteSpace(_secretId) ||
                string.IsNullOrWhiteSpace(_secretKey))
            {
                throw new Exception("Missing configuration parameters");
            }


            CosXmlConfig config = new CosXmlConfig.Builder()
                  .IsHttps(true)  //设置默认 HTTPS 请求
                  .SetRegion(_region)  //设置一个默认的存储桶地域
                                       //.SetDebugLog(true)  //显示日志
                  .Build();  //创建 CosXmlConfig 对象

            long durationSecond = 600;  //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(
              _secretId, _secretKey, durationSecond);

            _cosXml = new CosXmlServer(config, cosCredentialProvider);
        }


        public static async Task<string> UploadFile(string filePath)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_cosXml, transferConfig);

            string bucket = _bucket; //存储桶，格式：BucketName-APPID

            var fileName = Path.GetFileName(filePath);
            var dirName = Path.GetFileName(Path.GetDirectoryName(filePath));

            string cosPath = $@"EvernoteToNotion/{dirName}/{fileName}"; //对象在存储桶中的位置标识符，即称对象键
            string srcPath = filePath;//本地文件绝对路径

            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(bucket, cosPath);
            uploadTask.SetSrcPath(srcPath);

            uploadTask.progressCallback = delegate (long completed, long total)
            {
                //Console.WriteLine(string.Format("progress = {0:##.##}%", completed * 100.0 / total));
            };

            string uploadResultUrl = "";

            try
            {
                COSXML.Transfer.COSXMLUploadTask.UploadTaskResult result = await transferManager.UploadAsync(uploadTask);
                //Console.WriteLine(result.GetResultInfo());
                string eTag = result.eTag;

                uploadResultUrl = $@"https://{_bucket}.cos.{_region}.myqcloud.com/{cosPath}";

                Console.WriteLine(@$"上传成功：{uploadResultUrl}");
            }
            catch (Exception e)
            {
                Console.WriteLine("CosException: " + e);
            }
            return uploadResultUrl;
        }

    }
}
