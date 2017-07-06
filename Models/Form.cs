using System.Collections.Generic;

namespace PdfMagic.Model
{
    public class Form
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public List<FormField> Fields { get; set; }
    }
}
