// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Backend.Models;
// using FirstProjectWithMVC.Models;
// using FirstProjectWithMVC.ViewModels;

// namespace FirstProjectWithMVC.Repository.School
// {
//     public class DivisionRepository : IDivisionRepository
//     {
//         private readonly DataContext context;

//         public DivisionRepository(DataContext _context)
//         {
//             context = _context;

//         }
//         public void Add(DivisionViewModel obj)
//         {
//             Division newClass = new Division
//             {
//                 DivisionName = obj.DivisionName,
//                 ClassID = obj.ClassID
//             };

//             try
//             {
//                 context.Divisions.Add(newClass);
//                 context.SaveChanges();
//             }
//             catch (Exception ex)
//             {
//                 // Log the exception (you can use any logging library here)
//                 Console.WriteLine($"Error adding class: {ex.Message}");
//                 throw; // Re-throw or handle as needed
//             }
//         }
//         public void ChangeState(int id, bool state)
//         {
//             var existingDivision = GetById(id);  // Fetch the division by its ID
//             if (existingDivision != null)
//             {
//                 existingDivision.Active = state;  // Update the Active property
//                 context.SaveChanges();            // Save changes to the database
//             }
//         }


//         public void Delete(int id)
//         {
//             var existingDivision = GetById(id);
//             if (existingDivision != null)
//             {
//                 context.Divisions.Remove(existingDivision);
//                 context.SaveChanges();
//             }
//         }

//         public List<DivisionViewModel> DisplayDivisiones()
//         {
//             var divisions = context.Divisions
//                 .Include(d => d.Class)
//                 .Select(d => new DivisionViewModel
//                 {
//                     DivisionID = d.DivisionID,
//                     DivisionName = d.DivisionName,
//                     ClassID = d.ClassID,
//                     ClassesName = d.Class.ClassName,  // Access the class name
//                     StudentCount = d.Students != null ? d.Students.Count() : 0,
//                     Active = d.Active,
//                 }).ToList();

//             return divisions;
//         }



//         public Division GetById(int id)
//         {

//             return context.Divisions.FirstOrDefault(d => d.DivisionID == id)!;
//         }

//         public void Update(DivisionViewModel model)
//         {
//             var existingDivision = context.Divisions.FirstOrDefault(d => d.DivisionID == model.DivisionID);
//             if (existingDivision != null)
//             {
//                 existingDivision.DivisionName = model.DivisionName;
//                 existingDivision.ClassID = model.ClassID;
                
//                 context.SaveChanges();
//             }
//         }
//     }
// }