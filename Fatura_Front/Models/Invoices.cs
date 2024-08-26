using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fatura_Front.Models
{
    public class Invoices
    {
        public int InvoiceID { get; set; }
        public int CustomerID { get; set; }
        public string InvoicePath { get; set; }

        public Boolean printed { get; set; }
     
    }
}
