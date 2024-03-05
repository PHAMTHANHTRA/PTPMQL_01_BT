using System.ComponentModel.DataAnnotations;
using DemoMVC.Models;
using Microsoft.JSInterop.Infrastructure;
//Phạm Thanh Trà _ 2021050646
namespace MvcMovie.Models; 
    public class EmployeeModel : Person
    {
        
       public string EmployeeID  { get; set; }
       public int Age  { get; set; }
       
    }