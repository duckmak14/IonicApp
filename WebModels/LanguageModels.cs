using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace WebModels
{
    [Serializable]
    public partial class Language
    {
        public Language()
        {                                                          
        }
        [Key]
        public string ID { get; set; }
        public string Title { get; set; }
        public Nullable<bool> Published { get; set; }
        public Nullable<int> Order { get; set; }                                     
    }
    
}
