using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeMicroservice.Controllers;

namespace HomeMicroservice.Models{

public class AttendanceResponse{
   public IEnumerable<Person> people {get; set;}
}

}