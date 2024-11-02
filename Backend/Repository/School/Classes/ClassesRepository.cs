using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Data;
using Backend.DTOS;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School
{
    public class ClassesRepository : IClassesRepository
    {
        private readonly DatabaseContext context;

        public ClassesRepository(DatabaseContext _context)
        {
            context = _context;
        }

        public void Add(AddClassDTO obj)
        {
            Class newClass = new Class
            {
                ClassName = obj.ClassName,
                ClassYear = obj.ClassYear,
                StageID = obj.StageID
            };

            try
            {
                context.Classes.Add(newClass);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging library here)
                Console.WriteLine($"Error adding class: {ex.Message}");
                throw; // Re-throw or handle as needed
            }
        }

        public void Update(AddClassDTO model)
        {
            var existingClass = context.Classes.FirstOrDefault(c => c.ClassID == model.ClassID); // Fetch the class by ID
            if (existingClass != null)
            {
                existingClass.ClassName = model.ClassName;
                existingClass.StageID = model.StageID; // Update the StageID if needed

                context.Entry(existingClass).State = EntityState.Modified; // Mark entity as modified
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var existingClass = GetById(id);
            if (existingClass != null)
            {
                context.Classes.Remove(existingClass);
                context.SaveChanges();
            }
        }

        public List<Class> GetAll()
        {
            return context.Classes.ToList(); // Implemented to fetch all classes
        }

        public Class GetById(int id)
        {
            return context.Classes.FirstOrDefault(c => c.ClassID == id);
        }


        public List<AddClassDTO> DisplayClasses()
        {
            var classes = context.Classes
                .Select(c => new AddClassDTO
                {
                    ClassID = c.ClassID,
                    ClassName = c.ClassName,
                    ClassYear = c.ClassYear,
                    StageID = c.StageID, // Include StageID for the view
                    Stage = c.Stage,  // Fetch the single Stage object
                    Divisions = c.Divisions.ToList(),
                    StudentCount = c.StudentClass != null ? c.StudentClass.Count() : 0 // Manual null check
                }).ToList();

            return classes;
        }



    }
}
