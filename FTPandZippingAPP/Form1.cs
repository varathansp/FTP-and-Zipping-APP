using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace FTPandZippingAPP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            Form1 obj = new Form1(); 
            string[] files = obj.GetFileList();
            foreach (string file in files)
            {
                obj.Download(file);
            }
            obj.Zip();
        }
        public string[] GetFileList()
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + txtHostName.Text + "/" + txtDir.Text + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(txtUser.Text, txtPass.Text);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                reqFTP.Proxy = null;
                reqFTP.KeepAlive = false;
                reqFTP.UsePassive = false;
                response = reqFTP.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                // to remove the trailing '\n'
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                downloadFiles = null;
                return downloadFiles;
            }
        }

        private void Download(string file)
        {
            string remoteDir = txtDir.Text;
            try
            {
                string uri = "ftp://" + txtHostName.Text + "/" + remoteDir + "/" + file;
                Uri serverUri = new Uri(uri);
                if (serverUri.Scheme != Uri.UriSchemeFtp)
                {
                    return;
                }
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + txtHostName.Text + "/" + remoteDir + "/" + file));
                reqFTP.Credentials = new NetworkCredential("test", "test");
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = false;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                string path = txtLocalPath.Text + remoteDir;
                try
                {
                    if (!Directory.Exists(path))
                    {
                        // Try to create the directory.
                        DirectoryInfo di = Directory.CreateDirectory(path);
                    }
                }
                catch (IOException ioex)
                {
                    //MessageBox.Show(ioex.Message);
                    Console.WriteLine(ioex.Message);
                }
                FileStream writeStream = new FileStream(txtLocalPath.Text + remoteDir + "\\" + file, FileMode.Create);
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                writeStream.Close();
                response.Close();
            }
            catch (WebException wEx)
            {
                // MessageBox.Show(wEx.Message, "Download Error");
                Console.WriteLine(wEx.Message);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Download Error");
                Console.WriteLine(ex.Message);
            }
        }

        private void Zip()
        {
            string name = txtDir.Text;


            //string path = txtFolder.Text;
            // string path = "C:\\Users\\chsuj\\Desktop\\ProjectSample";

            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
            {
                zip.AddDirectory(txtLocalPath.Text + name);//File to Zip

                // System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

                // zip.SaveProgress += Zip_SaveProgress;

                zip.Save(txtLocalPath.Text + name + ".zip");


            }


        }
    }
}
