using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models
{
   [Table("Customer")]
     public class Customer  {
       
       [Key]
       public string? CustomerID { get; set; } 
       public string? FullName { get; set; }       

     }     

}