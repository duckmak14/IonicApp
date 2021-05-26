using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WebModels
{
    [DataContract]
    public abstract partial class BaseWebContent : BaseEntity
    {
        [DataMember]
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [DataMember]
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
        [DataMember]
        [Display(ResourceType = typeof(WebResources), Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [DataMember]
        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [DataMember]
        [Display(ResourceType = typeof(WebResources), Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }

        [DataMember]
        [DataType(DataType.DateTime)]
        [Display(ResourceType = typeof(WebResources), Name = "ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }
    [DataContract]
    public abstract partial class BaseEntity
    {
        
        [DataMember]
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; } 

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }
        private static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.ID, default(int));
        }
      

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(ID, other.ID))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(ID, default(int)))
                return base.GetHashCode();
            return ID.GetHashCode();
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
        
    } 
     
}
