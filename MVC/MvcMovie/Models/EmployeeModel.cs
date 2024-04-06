using System.ComponentModel.DataAnnotations;
using MvcMovie.Models;
using Microsoft.JSInterop.Infrastructure;
using MvcMovie.Controllers;
//Phạm Thanh Trà _ 2021050646
namespace MvcMovie.Models; 
    public class EmployeeModel : PersonModel
    {
        
       public string EmployeeID  { get; set; }
       public int Age  { get; set; }
       
    }

public class PersonModel
{
}