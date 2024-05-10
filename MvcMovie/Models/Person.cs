using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstMVC.Models
{
   [Table("Person")]
     public class Person  {
       
       [Key]
       public string? PersonID { get; set; } 
        public string? FullName { get; set; }   
        public string? Address { get; set; }    

     }     

}