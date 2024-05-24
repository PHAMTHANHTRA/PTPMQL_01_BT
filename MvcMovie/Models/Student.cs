using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
namespace MvcMovie.Models
{
     [Table("Students")]
     public class Student  {
       [Key]
       public string StudentID { get; set; } 
       public string FullName { get; set; }
        
        
       

     }     

}