using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PdfMagic.Model;
using System.Net;

namespace PdfMagicServiceCore.Controllers
{
    [Route("api/[controller]")]
    public class PdfMagicController : Controller
    {

        [HttpPost]
        [Route("fill")]
        public HttpResponseMessage Fill(Form form)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                const string pdfkPath = "pdftk.exe";
                string fdf = "%FDF-1.2 \n 1 0 obj \n <</FDF << /Fields[\n";

                foreach (FormField fld in form.Fields)
                {
                    fdf += "<< /T(" + fld.FieldName + ")/V(" + fld.FieldValue + ")>>\n ";
                }

                fdf += "] >> >> \n";
                fdf += "endobj \n";
                fdf += "trailer \n";
                fdf += "<</Root 1 0 R>> \n";
                fdf += "%% EOF \n";

                System.IO.File.WriteAllText(@"Forms\\data.fdf", fdf);

                switch (form.Id)
                {
                    case 5:
                        string a = @"Forms\\SAR7_NP.pdf fill_form Forms\\data.fdf output Forms\\sar7_filled.pdf";
                        Process p = new Process();
                        p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                        p.StartInfo.FileName = pdfkPath;
                        p.StartInfo.Arguments = a;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        p.WaitForExit();
                        break;
                }

                //ControllerBase.File(byte[] fileContents, string contentType)

                MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes("Forms\\sar7_filled.pdf"));

                // processing the stream.

                result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };

                result.Content.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "filled.pdf"
                    };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        
        [HttpGet]
        [Route("getform/{Id}")]
        public Form GetForm(int Id)
        {
            Form form = new Form();
            switch (Id)
            {
                case 1:
                    form.Id = 1;
                    form.Name = "1040";
                    form.FileName = "f1040.pdf";
                    form.Fields = GetFields(form.FileName);
                    break;
                case 2:
                    form.Id = 2;
                    form.Name = "1040-EZ";
                    form.FileName = "f1040ez.pdf";
                    form.Fields = GetFields(form.FileName);
                    break;
                case 3:
                    form.Id = 3;
                    form.Name = "W-2";
                    form.FileName = "fw2.pdf";
                    form.Fields = GetFields(form.FileName);
                    break;
                case 4:
                    form.Id = 4;
                    form.Name = "W-4";
                    form.FileName = "fw4.pdf";
                    form.Fields = GetFields(form.FileName);
                    break;
                case 5:
                    form.Id = 5;
                    form.Name = "SAR7";
                    form.FileName = "SAR7_NP.pdf";
                    form.Fields = GetFields(form.FileName);
                    break;
            }

            return form;
        }

        [HttpGet]
        [Route("getforms")]
        public List<Form> GetForms()
        {
            List<Form> forms = new List<Form>();
            Form f1040 = new Form();
            f1040.Id = 1;
            f1040.Name = "1040";
            f1040.FileName = "f1040.pdf";
            f1040.Fields = GetFields(f1040.FileName);

            Form f1040ez = new Form();
            f1040ez.Id = 2;
            f1040ez.Name = "1040-EZ";
            f1040ez.FileName = "f1040ez.pdf";
            f1040ez.Fields = GetFields(f1040ez.FileName);

            Form fw2 = new Form();
            fw2.Id = 3;
            fw2.Name = "W-2";
            fw2.FileName = "fw2.pdf";
            fw2.Fields = GetFields(fw2.FileName);

            Form fw4 = new Form();
            fw4.Id = 4;
            fw4.Name = "W-4";
            fw4.FileName = "fw4.pdf";
            fw4.Fields = GetFields(fw4.FileName);

            Form sar7 = new Form();
            sar7.Id = 5;
            sar7.Name = "SAR7";
            sar7.FileName = "SAR7_NP.pdf";
            sar7.Fields = GetFields(sar7.FileName);

            forms.Add(f1040);
            forms.Add(f1040ez);
            forms.Add(fw2);
            forms.Add(fw4);
            forms.Add(sar7);

            return forms;
        }

/*
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
 */

        private List<FormField> GetFields(string fileName)
        {
            List<FormField> fields = new List<FormField>();
            string[] rawFields;

            const string pdfkPath = "pdftk.exe";

            string a = "Forms\\" + fileName + " dump_data_fields";


            Process p = new Process();
            p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            p.StartInfo.FileName = pdfkPath;
            p.StartInfo.Arguments = a;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            string[] delim = { "---" };

            StringBuilder q = new StringBuilder();
            while (!p.HasExited)
            {
                q.Append(p.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();
            rawFields = r.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            p.WaitForExit();

            string regexFieldType = "(?<=FieldType:)((.|\n)*)(?=FieldName)";
            string regexFieldName = "(?<=FieldName:)((.|\n)*)(?=FieldFlags)";

            foreach (string fieldStr in rawFields)
            {
                Match mFieldType = Regex.Match(fieldStr, regexFieldType);
                Match mFieldName = Regex.Match(fieldStr, regexFieldName);

                FormField fld = new FormField();
                fld.FieldType = mFieldType.Value.Trim();
                fld.FieldName = mFieldName.Value.Trim();
                fields.Add(fld);
            }
            return fields;
        }
    }
}
